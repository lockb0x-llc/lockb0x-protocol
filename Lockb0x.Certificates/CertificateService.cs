using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Formats.Cbor;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using Lockb0x.Certificates.Models;
using Lockb0x.Certificates.Stores;
using Lockb0x.Core.Canonicalization;
using Lockb0x.Core.Models;
using Lockb0x.Core.Utilities;
using Lockb0x.Core.Validation;
using Lockb0x.Signing;

namespace Lockb0x.Certificates;

/// <summary>
/// Reference implementation of the Lockb0x certificate issuance and validation workflow.
/// </summary>
public sealed class CertificateService : ICertificateService
{
    private const string EntryHashOid = "1.3.6.1.4.1.58543.100.1";
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = null,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    private readonly IJsonCanonicalizer _canonicalizer;
    private readonly ISigningService _signingService;
    private readonly ICodexEntryValidator _validator;
    private readonly ICertificateStore _store;
    private readonly HashAlgorithmName _hashAlgorithm;

    public CertificateService()
        : this(new JcsCanonicalizer(), new JoseCoseSigningService(), new CodexEntryValidator(), new InMemoryCertificateStore(), HashAlgorithmName.SHA256)
    {
    }

    public CertificateService(
        IJsonCanonicalizer canonicalizer,
        ISigningService signingService,
        ICodexEntryValidator validator,
        ICertificateStore store,
        HashAlgorithmName? hashAlgorithm = null)
    {
        _canonicalizer = canonicalizer ?? throw new ArgumentNullException(nameof(canonicalizer));
        _signingService = signingService ?? throw new ArgumentNullException(nameof(signingService));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _hashAlgorithm = hashAlgorithm ?? HashAlgorithmName.SHA256;
    }

    public async Task<CertificateDescriptor> IssueCertificateAsync(CodexEntry entry, CertificateOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.SigningKey);

        var validation = _validator.Validate(entry);
        if (!validation.Success)
        {
            var errors = string.Join(", ", validation.Errors.Select(e => $"{e.Code}:{e.Message}"));
            throw new InvalidOperationException($"Codex entry failed validation and cannot be certified: {errors}");
        }

        if (entry.Signatures is null || entry.Signatures.Count == 0)
        {
            throw new InvalidOperationException("Codex entries must contain signatures before certificate issuance.");
        }

        var issuer = options.Issuer ?? entry.Identity.Org;
        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new InvalidOperationException("A certificate issuer must be provided via options or entry identity.");
        }

        var subject = options.Subject ?? entry.Identity.Subject ?? entry.Id;
        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new InvalidOperationException("A certificate subject could not be derived from the Codex entry.");
        }

        if (string.IsNullOrWhiteSpace(options.SigningKey.KeyId))
        {
            throw new InvalidOperationException("Signing keys must include a stable key identifier (kid).");
        }

        if (string.IsNullOrWhiteSpace(options.SigningKey.PrivateKey))
        {
            throw new InvalidOperationException("Signing keys must include private key material for issuance operations.");
        }

        var certificateId = !string.IsNullOrWhiteSpace(options.CertificateId)
            ? options.CertificateId!
            : $"urn:uuid:{Guid.NewGuid()}";

        var issuedAt = options.IssuedAt ?? DateTimeOffset.UtcNow;
        var entryHashBytes = _canonicalizer.Hash(entry, _hashAlgorithm);
        var entryHash = NiUri.Create(entryHashBytes, _hashAlgorithm);

        var representations = new List<CertificateRepresentation>();
        var formats = options.Formats?.Distinct().ToList() ?? new List<CertificateFormat>();
        if (formats.Count == 0)
        {
            throw new InvalidOperationException("At least one certificate format must be requested.");
        }

        foreach (var format in formats)
        {
            cancellationToken.ThrowIfCancellationRequested();
            switch (format)
            {
                case CertificateFormat.Json:
                    representations.Add(await CreateJsonRepresentationAsync(entry, options, certificateId, issuer, subject, issuedAt, entryHash).ConfigureAwait(false));
                    break;
                case CertificateFormat.VerifiableCredential:
                    representations.Add(await CreateVerifiableCredentialRepresentationAsync(entry, options, certificateId, issuer, subject, issuedAt, entryHash).ConfigureAwait(false));
                    break;
                case CertificateFormat.Jwt:
                    representations.Add(await CreateJwtRepresentationAsync(entry, options, certificateId, issuer, subject, issuedAt, entryHash).ConfigureAwait(false));
                    break;
                case CertificateFormat.Cwt:
                    representations.Add(await CreateCwtRepresentationAsync(entry, options, certificateId, issuer, subject, issuedAt, entryHash, entryHashBytes).ConfigureAwait(false));
                    break;
                case CertificateFormat.X509:
                    representations.Add(CreateX509Representation(entry, options, certificateId, issuer, subject, issuedAt, entryHashBytes));
                    break;
                default:
                    throw new NotSupportedException($"Unsupported certificate format '{format}'.");
            }
        }

        var descriptor = new CertificateDescriptor(
            certificateId,
            entry.Id,
            issuer,
            subject,
            options.Purpose,
            issuedAt,
            options.ExpiresAt,
            CertificateStatus.Active,
            new ReadOnlyCollection<CertificateRepresentation>(representations),
            new ReadOnlyCollection<CertificateEvent>(new List<CertificateEvent>
            {
                new("issued", issuedAt, "Certificate issued.")
            }));

        await _store.SaveAsync(descriptor, cancellationToken).ConfigureAwait(false);
        return descriptor;
    }

    public async Task<CertificateValidationResult> ValidateCertificateAsync(CertificateDescriptor certificate, CodexEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(certificate);
        ArgumentNullException.ThrowIfNull(entry);

        var result = new CertificateValidationResult();
        if (!string.Equals(certificate.EntryId, entry.Id, StringComparison.Ordinal))
        {
            return result.AddError("Certificate entry id does not match supplied Codex entry.");
        }

        if (certificate.Status == CertificateStatus.Revoked)
        {
            result.AddError("Certificate has been revoked.");
        }

        if (certificate.ExpiresAt is not null && certificate.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            result.AddError("Certificate has expired.");
        }

        var validation = _validator.Validate(entry);
        if (!validation.Success)
        {
            foreach (var error in validation.Errors)
            {
                result.AddError($"Codex entry invalid: {error.Code} - {error.Message}");
            }
        }

        if (!result.Success)
        {
            return result;
        }

        var entryHashBytes = _canonicalizer.Hash(entry, _hashAlgorithm);
        var entryHash = NiUri.Create(entryHashBytes, _hashAlgorithm);

        foreach (var representation in certificate.Representations)
        {
            cancellationToken.ThrowIfCancellationRequested();
            switch (representation)
            {
                case JsonCertificateRepresentation json:
                    ValidateEntryHash(json.EntryHash, entryHash, result);
                    try
                    {
                        await ValidateJsonRepresentationAsync(json, cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        result.AddError($"JSON certificate validation failed: {ex.Message}");
                    }
                    break;
                case VerifiableCredentialRepresentation vc:
                    ValidateEntryHash(vc.EntryHash, entryHash, result);
                    try
                    {
                        await ValidateVerifiableCredentialAsync(vc, cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        result.AddError($"Verifiable credential validation failed: {ex.Message}");
                    }
                    break;
                case JwtCertificateRepresentation jwt:
                    ValidateEntryHash(jwt.EntryHash, entryHash, result);
                    try
                    {
                        await ValidateJwtAsync(jwt, cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        result.AddError($"JWT validation failed: {ex.Message}");
                    }
                    break;
                case CwtCertificateRepresentation cwt:
                    ValidateEntryHash(cwt.EntryHash, entryHash, result);
                    try
                    {
                        await ValidateCwtAsync(cwt, entryHashBytes, cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        result.AddError($"CWT validation failed: {ex.Message}");
                    }
                    break;
                case X509CertificateRepresentation x509:
                    ValidateX509(x509, entryHashBytes, result);
                    break;
            }
        }

        return result;
    }

    public async Task<bool> RevokeCertificateAsync(string certificateId, string? reason = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(certificateId))
        {
            return false;
        }

        var existing = await _store.GetAsync(certificateId, cancellationToken).ConfigureAwait(false);
        if (existing is null)
        {
            return false;
        }

        if (existing.Status == CertificateStatus.Revoked)
        {
            return true;
        }

        var revokedAt = DateTimeOffset.UtcNow;
        var updated = existing.WithStatus(
            CertificateStatus.Revoked,
            new[] { new CertificateEvent("revoked", revokedAt, reason ?? "Certificate revoked.") });

        await _store.UpdateAsync(updated, cancellationToken).ConfigureAwait(false);
        return true;
    }

    public Task<CertificateDescriptor?> GetCertificateAsync(string certificateId, CancellationToken cancellationToken = default)
        => _store.GetAsync(certificateId, cancellationToken);

    private async Task<JsonCertificateRepresentation> CreateJsonRepresentationAsync(
        CodexEntry entry,
        CertificateOptions options,
        string certificateId,
        string issuer,
        string subject,
        DateTimeOffset issuedAt,
        string entryHash)
    {
        var document = new JsonCertificateDocument
        {
            CertificateType = "lockb0x-json",
            CertificateId = certificateId,
            CodexEntryId = entry.Id,
            ProtocolVersion = options.ProtocolVersion ?? entry.Version,
            IssuedAt = issuedAt,
            ExpiresAt = options.ExpiresAt,
            Issuer = issuer,
            Subject = subject,
            Purpose = options.Purpose.ToString().ToLowerInvariant(),
            EntryHash = entryHash,
            Identity = entry.Identity,
            Encryption = entry.Encryption,
            Storage = entry.Storage,
            Anchor = entry.Anchor,
            Signatures = entry.Signatures,
            AdditionalMetadata = options.AdditionalMetadata
        };

        var canonical = _canonicalizer.Canonicalize(document);
        var signature = await _signingService.SignAsync(Encoding.UTF8.GetBytes(canonical), options.SigningKey, options.SigningAlgorithm).ConfigureAwait(false);
        var json = Serialize(document);
        return new JsonCertificateRepresentation(json, canonical, signature, document.ProtocolVersion, entryHash);
    }

    private async Task<VerifiableCredentialRepresentation> CreateVerifiableCredentialRepresentationAsync(
        CodexEntry entry,
        CertificateOptions options,
        string certificateId,
        string issuer,
        string subject,
        DateTimeOffset issuedAt,
        string entryHash)
    {
        var credentialSubject = new CredentialSubjectDocument
        {
            Id = subject,
            CodexEntryId = entry.Id,
            EntryHash = entryHash,
            Purpose = options.Purpose.ToString().ToLowerInvariant(),
            Anchor = entry.Anchor,
            Storage = entry.Storage,
            Encryption = entry.Encryption,
            Identity = entry.Identity
        };

        var vcWithoutProof = new VerifiableCredentialDocument
        {
            Context = options.VerifiableCredentialContexts.Select(c => (object)c).ToList(),
            Id = certificateId,
            Type = new List<string> { "VerifiableCredential", "Lockb0xCertificate" },
            Issuer = issuer,
            IssuanceDate = issuedAt,
            ExpirationDate = options.ExpiresAt,
            CredentialSubject = credentialSubject
        };

        var canonical = _canonicalizer.Canonicalize(vcWithoutProof);
        var canonicalSignature = await _signingService.SignAsync(Encoding.UTF8.GetBytes(canonical), options.SigningKey, options.SigningAlgorithm).ConfigureAwait(false);
        var jws = await CreateJwsAsync(canonicalSignature.ProtectedHeader, canonical, options, "vc+jwt").ConfigureAwait(false);

        var proof = new VerifiableCredentialProof
        {
            Type = "JsonWebSignature2020",
            Created = issuedAt,
            ProofPurpose = "assertionMethod",
            VerificationMethod = canonicalSignature.ProtectedHeader.KeyId ?? options.SigningKey.KeyId ?? issuer,
            Jws = jws
        };

        var vc = vcWithoutProof with { Proof = proof };
        var credential = Serialize(vc);
        return new VerifiableCredentialRepresentation(credential, canonical, canonicalSignature, options.VerifiableCredentialContexts, entryHash, jws);
    }

    private async Task<JwtCertificateRepresentation> CreateJwtRepresentationAsync(
        CodexEntry entry,
        CertificateOptions options,
        string certificateId,
        string issuer,
        string subject,
        DateTimeOffset issuedAt,
        string entryHash)
    {
        var audience = options.Audience ?? "lockb0x";
        var payload = new JwtPayloadDocument
        {
            Issuer = issuer,
            Subject = subject,
            Audience = audience,
            IssuedAt = issuedAt.ToUnixTimeSeconds(),
            ExpiresAt = options.ExpiresAt?.ToUnixTimeSeconds(),
            TokenId = certificateId,
            EntryId = entry.Id,
            EntryHash = entryHash,
            Anchor = entry.Anchor,
            Storage = entry.Storage,
            Purpose = options.Purpose.ToString().ToLowerInvariant()
        };

        var payloadJson = Serialize(payload);
        var payloadB64 = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(payloadJson));

        var header = new JwtHeaderDocument
        {
            Algorithm = options.SigningAlgorithm,
            Type = "JWT",
            KeyId = options.SigningKey.KeyId
        };

        var headerJson = Serialize(header);
        var headerB64 = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(headerJson));
        var signingInput = Encoding.UTF8.GetBytes($"{headerB64}.{payloadB64}");
        var signature = await _signingService.SignAsync(signingInput, options.SigningKey, options.SigningAlgorithm).ConfigureAwait(false);
        var token = $"{headerB64}.{payloadB64}.{signature.Signature}";
        return new JwtCertificateRepresentation(token, headerJson, payloadJson, signature, entryHash);
    }

    private async Task<CwtCertificateRepresentation> CreateCwtRepresentationAsync(
        CodexEntry entry,
        CertificateOptions options,
        string certificateId,
        string issuer,
        string subject,
        DateTimeOffset issuedAt,
        string entryHash,
        byte[] entryHashBytes)
    {
        var writer = new CborWriter();
        writer.WriteStartMap(11);
        writer.WriteInt32(1); // iss
        writer.WriteTextString(issuer);
        writer.WriteInt32(2); // sub
        writer.WriteTextString(subject);
        writer.WriteInt32(3); // aud
        writer.WriteTextString(options.Audience ?? "lockb0x");
        var expires = options.ExpiresAt ?? issuedAt.AddYears(1);
        writer.WriteInt32(4); // exp
        writer.WriteInt64(expires.ToUnixTimeSeconds());
        writer.WriteInt32(5); // nbf
        writer.WriteInt64(issuedAt.ToUnixTimeSeconds());
        writer.WriteInt32(6); // iat
        writer.WriteInt64(issuedAt.ToUnixTimeSeconds());
        writer.WriteInt32(7); // cti
        writer.WriteTextString(certificateId);
        writer.WriteInt32(1000); // Lockb0x entry hash (custom claim)
        writer.WriteByteString(entryHashBytes);
        writer.WriteInt32(1001); // Anchor chain
        writer.WriteTextString(entry.Anchor.Chain);
        writer.WriteInt32(1002); // Anchor transaction hash
        writer.WriteTextString(entry.Anchor.TransactionHash);
        writer.WriteInt32(1003); // Purpose
        writer.WriteTextString(options.Purpose.ToString().ToLowerInvariant());
        writer.WriteEndMap();

        var payload = writer.Encode();
        var signature = await _signingService.SignAsync(payload, options.SigningKey, options.SigningAlgorithm).ConfigureAwait(false);
        return new CwtCertificateRepresentation(payload, signature, entryHash);
    }

    private X509CertificateRepresentation CreateX509Representation(
        CodexEntry entry,
        CertificateOptions options,
        string certificateId,
        string issuer,
        string subject,
        DateTimeOffset issuedAt,
        byte[] entryHashBytes)
    {
        using var rsa = RSA.Create(2048);
        var distinguishedName = CreateDistinguishedName(subject);
        var issuerName = CreateDistinguishedName(issuer);
        var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
        request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

        var writer = new System.Formats.Asn1.AsnWriter(System.Formats.Asn1.AsnEncodingRules.DER);
        writer.WriteOctetString(entryHashBytes);
        request.CertificateExtensions.Add(new X509Extension(new Oid(EntryHashOid), writer.Encode(), critical: true));

        var notBefore = issuedAt.AddMinutes(-5);
        var notAfter = options.ExpiresAt ?? issuedAt.AddYears(1);
        using var certificate = request.CreateSelfSigned(notBefore, notAfter);
        if (OperatingSystem.IsWindows())
        {
            certificate.FriendlyName = $"Lockb0x Certificate {certificateId}";
        }
        return new X509CertificateRepresentation(certificate.Export(X509ContentType.Cert), _hashAlgorithm.Name.ToLowerInvariant(), entryHashBytes);
    }

    private static X500DistinguishedName CreateDistinguishedName(string value)
    {
        var escaped = value.Replace(",", "+", StringComparison.Ordinal).Replace("\"", "'", StringComparison.Ordinal);
        return new X500DistinguishedName($"CN={escaped}");
    }

    private static string Serialize<T>(T value) => JsonSerializer.Serialize(value, SerializerOptions);

    private static void ValidateEntryHash(string representationHash, string expectedHash, CertificateValidationResult result)
    {
        if (!string.Equals(representationHash, expectedHash, StringComparison.Ordinal))
        {
            result.AddError("Certificate representation hash does not match Codex entry hash.");
        }
    }

    private async Task ValidateJsonRepresentationAsync(JsonCertificateRepresentation representation, CancellationToken cancellationToken)
    {
        var canonicalBytes = Encoding.UTF8.GetBytes(representation.CanonicalForm);
        if (!await _signingService.VerifyAsync(canonicalBytes, representation.Signature).ConfigureAwait(false))
        {
            throw new InvalidOperationException("JSON certificate signature verification failed.");
        }
    }

    private async Task ValidateVerifiableCredentialAsync(VerifiableCredentialRepresentation representation, CancellationToken cancellationToken)
    {
        var canonicalBytes = Encoding.UTF8.GetBytes(representation.CanonicalForm);
        if (!await _signingService.VerifyAsync(canonicalBytes, representation.Proof).ConfigureAwait(false))
        {
            throw new InvalidOperationException("Verifiable credential proof validation failed.");
        }

        if (!await ValidateJwsAsync(representation.Jws, canonicalBytes).ConfigureAwait(false))
        {
            throw new InvalidOperationException("Verifiable credential JWS could not be validated.");
        }
    }

    private async Task ValidateJwtAsync(JwtCertificateRepresentation representation, CancellationToken cancellationToken)
    {
        var parts = representation.Token.Split('.');
        if (parts.Length != 3)
        {
            throw new InvalidOperationException("JWT representation is malformed.");
        }

        var signingInput = Encoding.UTF8.GetBytes($"{parts[0]}.{parts[1]}");
        var signature = new SignatureProof
        {
            ProtectedHeader = new SignatureProtectedHeader
            {
                Algorithm = representation.Signature.ProtectedHeader.Algorithm,
                KeyId = representation.Signature.ProtectedHeader.KeyId
            },
            Signature = parts[2]
        };

        if (!await _signingService.VerifyAsync(signingInput, signature).ConfigureAwait(false))
        {
            throw new InvalidOperationException("JWT signature validation failed.");
        }
    }

    private async Task ValidateCwtAsync(CwtCertificateRepresentation representation, byte[] expectedEntryHash, CancellationToken cancellationToken)
    {
        var reader = new CborReader(representation.Payload);
        var mapLength = reader.ReadStartMap();
        byte[]? hash = null;
        if (mapLength is null)
        {
            while (reader.PeekState() != CborReaderState.EndMap)
            {
                var key = reader.ReadInt32();
                if (key == 1000)
                {
                    hash = reader.ReadByteString();
                }
                else
                {
                    reader.SkipValue();
                }
            }
        }
        else
        {
            for (var i = 0; i < mapLength.Value; i++)
            {
                var key = reader.ReadInt32();
                if (key == 1000)
                {
                    hash = reader.ReadByteString();
                }
                else
                {
                    reader.SkipValue();
                }
            }
        }
        reader.ReadEndMap();

        if (hash is null || !hash.AsSpan().SequenceEqual(expectedEntryHash))
        {
            throw new InvalidOperationException("CWT representation entry hash mismatch.");
        }

        if (!await _signingService.VerifyAsync(representation.Payload, representation.Signature).ConfigureAwait(false))
        {
            throw new InvalidOperationException("CWT signature validation failed.");
        }
    }

    private static void ValidateX509(X509CertificateRepresentation representation, byte[] expectedEntryHash, CertificateValidationResult result)
    {
        using var certificate = new X509Certificate2(representation.Certificate);
        var extension = certificate.Extensions[EntryHashOid];
        if (extension is null)
        {
            result.AddError("X.509 certificate missing Codex entry hash extension.");
            return;
        }

        var raw = extension.RawData;
        var reader = new System.Formats.Asn1.AsnReader(raw, System.Formats.Asn1.AsnEncodingRules.DER);
        var hash = reader.ReadOctetString();
        if (!hash.AsSpan().SequenceEqual(expectedEntryHash))
        {
            result.AddError("X.509 certificate Codex entry hash mismatch.");
        }

        if (certificate.NotAfter < DateTimeOffset.UtcNow)
        {
            result.AddError("X.509 certificate has expired.");
        }
    }

    private async Task<string> CreateJwsAsync(SignatureProtectedHeader headerTemplate, string canonical, CertificateOptions options, string? type)
    {
        var header = new JwsHeader
        {
            Algorithm = headerTemplate.Algorithm,
            KeyId = headerTemplate.KeyId ?? options.SigningKey.KeyId,
            Type = type
        };

        var headerJson = Serialize(header);
        var headerB64 = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(headerJson));
        var payloadB64 = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(canonical));
        var signingInput = Encoding.UTF8.GetBytes($"{headerB64}.{payloadB64}");
        var signature = await _signingService.SignAsync(signingInput, options.SigningKey, options.SigningAlgorithm).ConfigureAwait(false);
        return $"{headerB64}.{payloadB64}.{signature.Signature}";
    }

    private async Task<bool> ValidateJwsAsync(string token, byte[] canonicalBytes)
    {
        var parts = token.Split('.');
        if (parts.Length != 3)
        {
            return false;
        }

        var payload = Base64UrlEncoder.Decode(parts[1]);
        if (!payload.AsSpan().SequenceEqual(canonicalBytes))
        {
            return false;
        }

        var headerJson = Encoding.UTF8.GetString(Base64UrlEncoder.Decode(parts[0]));
        var header = JsonSerializer.Deserialize<JwsHeader>(headerJson, SerializerOptions);
        if (header is null)
        {
            return false;
        }

        var signature = new SignatureProof
        {
            ProtectedHeader = new SignatureProtectedHeader
            {
                Algorithm = header.Algorithm,
                KeyId = header.KeyId
            },
            Signature = parts[2]
        };

        return await _signingService.VerifyAsync(Encoding.UTF8.GetBytes($"{parts[0]}.{parts[1]}"), signature).ConfigureAwait(false);
    }

    private sealed record JsonCertificateDocument
    {
        [JsonPropertyName("certificate_type")]
        public required string CertificateType { get; init; }

        [JsonPropertyName("certificate_id")]
        public required string CertificateId { get; init; }

        [JsonPropertyName("codex_entry_id")]
        public required string CodexEntryId { get; init; }

        [JsonPropertyName("protocol_version")]
        public required string ProtocolVersion { get; init; }

        [JsonPropertyName("issued_at")]
        public required DateTimeOffset IssuedAt { get; init; }

        [JsonPropertyName("expires_at")]
        public DateTimeOffset? ExpiresAt { get; init; }

        [JsonPropertyName("issuer")]
        public required string Issuer { get; init; }

        [JsonPropertyName("subject")]
        public required string Subject { get; init; }

        [JsonPropertyName("purpose")]
        public required string Purpose { get; init; }

        [JsonPropertyName("entry_hash")]
        public required string EntryHash { get; init; }

        [JsonPropertyName("identity")]
        public required IdentityDescriptor Identity { get; init; }

        [JsonPropertyName("encryption")]
        public EncryptionDescriptor? Encryption { get; init; }

        [JsonPropertyName("storage")]
        public required StorageDescriptor Storage { get; init; }

        [JsonPropertyName("anchor")]
        public required AnchorProof Anchor { get; init; }

        [JsonPropertyName("signatures")]
        public required IReadOnlyList<SignatureProof> Signatures { get; init; }

        [JsonPropertyName("additional_metadata")]
        public IReadOnlyDictionary<string, string>? AdditionalMetadata { get; init; }
    }

    private sealed record CredentialSubjectDocument
    {
        [JsonPropertyName("id")]
        public required string Id { get; init; }

        [JsonPropertyName("codex_entry_id")]
        public required string CodexEntryId { get; init; }

        [JsonPropertyName("entry_hash")]
        public required string EntryHash { get; init; }

        [JsonPropertyName("purpose")]
        public required string Purpose { get; init; }

        [JsonPropertyName("identity")]
        public required IdentityDescriptor Identity { get; init; }

        [JsonPropertyName("storage")]
        public required StorageDescriptor Storage { get; init; }

        [JsonPropertyName("anchor")]
        public required AnchorProof Anchor { get; init; }

        [JsonPropertyName("encryption")]
        public EncryptionDescriptor? Encryption { get; init; }
    }

    private sealed record VerifiableCredentialDocument
    {
        [JsonPropertyName("@context")]
        public required List<object> Context { get; init; }

        [JsonPropertyName("id")]
        public required string Id { get; init; }

        [JsonPropertyName("type")]
        public required List<string> Type { get; init; }

        [JsonPropertyName("issuer")]
        public required string Issuer { get; init; }

        [JsonPropertyName("issuanceDate")]
        public required DateTimeOffset IssuanceDate { get; init; }

        [JsonPropertyName("expirationDate")]
        public DateTimeOffset? ExpirationDate { get; init; }

        [JsonPropertyName("credentialSubject")]
        public required CredentialSubjectDocument CredentialSubject { get; init; }

        [JsonPropertyName("proof")]
        public VerifiableCredentialProof? Proof { get; init; }
    }

    private sealed record VerifiableCredentialProof
    {
        [JsonPropertyName("type")]
        public required string Type { get; init; }

        [JsonPropertyName("created")]
        public required DateTimeOffset Created { get; init; }

        [JsonPropertyName("proofPurpose")]
        public required string ProofPurpose { get; init; }

        [JsonPropertyName("verificationMethod")]
        public required string VerificationMethod { get; init; }

        [JsonPropertyName("jws")]
        public required string Jws { get; init; }
    }

    private sealed record JwtHeaderDocument
    {
        [JsonPropertyName("alg")]
        public required string Algorithm { get; init; }

        [JsonPropertyName("typ")]
        public required string Type { get; init; }

        [JsonPropertyName("kid")]
        public string? KeyId { get; init; }
    }

    private sealed record JwtPayloadDocument
    {
        [JsonPropertyName("iss")]
        public required string Issuer { get; init; }

        [JsonPropertyName("sub")]
        public required string Subject { get; init; }

        [JsonPropertyName("aud")]
        public required string Audience { get; init; }

        [JsonPropertyName("iat")]
        public required long IssuedAt { get; init; }

        [JsonPropertyName("exp")]
        public long? ExpiresAt { get; init; }

        [JsonPropertyName("jti")]
        public required string TokenId { get; init; }

        [JsonPropertyName("codex_entry_id")]
        public required string EntryId { get; init; }

        [JsonPropertyName("entry_hash")]
        public required string EntryHash { get; init; }

        [JsonPropertyName("anchor")]
        public required AnchorProof Anchor { get; init; }

        [JsonPropertyName("storage")]
        public required StorageDescriptor Storage { get; init; }

        [JsonPropertyName("purpose")]
        public required string Purpose { get; init; }
    }

    private sealed record JwsHeader
    {
        [JsonPropertyName("alg")]
        public required string Algorithm { get; init; }

        [JsonPropertyName("kid")]
        public string? KeyId { get; init; }

        [JsonPropertyName("typ")]
        public string? Type { get; init; }
    }

    private static class Base64UrlEncoder
    {
        public static string Encode(ReadOnlySpan<byte> data)
        {
            var base64 = Convert.ToBase64String(data);
            return base64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        public static byte[] Decode(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Array.Empty<byte>();
            }

            var builder = new StringBuilder(value.Length);
            foreach (var c in value)
            {
                builder.Append(c switch
                {
                    '-' => '+',
                    '_' => '/',
                    _ => c
                });
            }

            var padded = builder.ToString();
            switch (padded.Length % 4)
            {
                case 2:
                    padded += "==";
                    break;
                case 3:
                    padded += "=";
                    break;
            }

            return Convert.FromBase64String(padded);
        }
    }
}
