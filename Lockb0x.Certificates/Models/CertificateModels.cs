using System.Collections.ObjectModel;
using System.Linq;
using Lockb0x.Core.Models;
using Lockb0x.Signing;

namespace Lockb0x.Certificates.Models;

/// <summary>
/// Enumerates the supported certificate formats defined by the Lockb0x protocol specification.
/// </summary>
public enum CertificateFormat
{
    Json,
    VerifiableCredential,
    Jwt,
    Cwt,
    X509
}

/// <summary>
/// Enumerates the supported certificate purposes.
/// </summary>
public enum CertificatePurpose
{
    Attestation,
    Provenance,
    Audit
}

/// <summary>
/// Represents the lifecycle status of a certificate.
/// </summary>
public enum CertificateStatus
{
    Active,
    Revoked,
    Expired
}

/// <summary>
/// Captures audit events recorded for a certificate.
/// </summary>
/// <param name="Type">A short machine readable event type (e.g. <c>issued</c>, <c>revoked</c>).</param>
/// <param name="Timestamp">The time at which the event occurred.</param>
/// <param name="Message">A human readable message describing the event.</param>
/// <param name="Actor">Optional identity of the actor who triggered the event.</param>
public sealed record CertificateEvent(string Type, DateTimeOffset Timestamp, string Message, string? Actor = null);

/// <summary>
/// Represents the immutable descriptor returned when issuing or retrieving a certificate.
/// </summary>
public sealed record CertificateDescriptor(
    string CertificateId,
    string EntryId,
    string Issuer,
    string Subject,
    CertificatePurpose Purpose,
    DateTimeOffset IssuedAt,
    DateTimeOffset? ExpiresAt,
    CertificateStatus Status,
    IReadOnlyList<CertificateRepresentation> Representations,
    IReadOnlyList<CertificateEvent> Events
)
{
    public CertificateDescriptor WithStatus(CertificateStatus status, IEnumerable<CertificateEvent>? additionalEvents = null)
    {
        if (status == Status && additionalEvents is null)
        {
            return this;
        }

        var events = Events.ToList();
        if (additionalEvents is not null)
        {
            events.AddRange(additionalEvents);
        }

        return this with
        {
            Status = status,
            Events = new ReadOnlyCollection<CertificateEvent>(events)
        };
    }
}

/// <summary>
/// Represents the inputs required to issue a certificate.
/// </summary>
public sealed class CertificateOptions
{
    private IReadOnlyCollection<CertificateFormat>? _formats;
    private IReadOnlyDictionary<string, string>? _additionalMetadata;
    private IReadOnlyCollection<string>? _vcContexts;

    /// <summary>
    /// Gets or sets the intended certificate purpose. Defaults to <see cref="CertificatePurpose.Attestation"/>.
    /// </summary>
    public CertificatePurpose Purpose { get; init; } = CertificatePurpose.Attestation;

    /// <summary>
    /// Gets or sets the formats to emit. Defaults to JSON, JWT, and Verifiable Credential.
    /// </summary>
    public IReadOnlyCollection<CertificateFormat> Formats
    {
        get => _formats ?? DefaultFormats;
        init => _formats = value ?? DefaultFormats;
    }

    /// <summary>
    /// Gets or sets the certificate issuer identifier. Defaults to <c>entry.Identity.Org</c>.
    /// </summary>
    public string? Issuer { get; init; }

    /// <summary>
    /// Gets or sets the certificate subject identifier. Defaults to <c>entry.Identity.Subject</c> or <c>entry.Id</c>.
    /// </summary>
    public string? Subject { get; init; }

    /// <summary>
    /// Gets or sets the optional audience claim for JWT/CWT representations.
    /// </summary>
    public string? Audience { get; init; }

    /// <summary>
    /// Gets or sets the certificate validity end time.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>
    /// Gets or sets the custom issuance time. Defaults to <see cref="DateTimeOffset.UtcNow"/>.
    /// </summary>
    public DateTimeOffset? IssuedAt { get; init; }

    /// <summary>
    /// Gets or sets the semantic protocol version string written into JSON certificates.
    /// Defaults to the Codex entry version when omitted.
    /// </summary>
    public string? ProtocolVersion { get; init; }

    /// <summary>
    /// Gets or sets optional additional metadata to embed into the JSON certificate representation.
    /// </summary>
    public IReadOnlyDictionary<string, string>? AdditionalMetadata
    {
        get => _additionalMetadata;
        init => _additionalMetadata = value is null ? null : new ReadOnlyDictionary<string, string>(value.ToDictionary(kv => kv.Key, kv => kv.Value));
    }

    /// <summary>
    /// Gets or sets the JSON-LD contexts for Verifiable Credential representations.
    /// </summary>
    public IReadOnlyCollection<string> VerifiableCredentialContexts
    {
        get => _vcContexts ?? DefaultVcContexts;
        init => _vcContexts = value ?? DefaultVcContexts;
    }

    /// <summary>
    /// Gets or sets the signing algorithm to use. Defaults to <c>EdDSA</c>.
    /// </summary>
    public string SigningAlgorithm { get; init; } = "EdDSA";

    /// <summary>
    /// Gets or sets the signing key used to create signatures across representations.
    /// </summary>
    public required SigningKey SigningKey { get; init; }

    /// <summary>
    /// Optional custom certificate identifier. When omitted, a UUIDv4 URN is generated.
    /// </summary>
    public string? CertificateId { get; init; }

    private static readonly IReadOnlyCollection<CertificateFormat> DefaultFormats = new ReadOnlyCollection<CertificateFormat>(new[]
    {
        CertificateFormat.Json,
        CertificateFormat.Jwt,
        CertificateFormat.VerifiableCredential
    });

    private static readonly IReadOnlyCollection<string> DefaultVcContexts = new ReadOnlyCollection<string>(new[]
    {
        "https://www.w3.org/2018/credentials/v1",
        "https://w3id.org/security/suites/ed25519-2020/v1",
        "https://lockb0x.org/contexts/lockb0x-certificate-v1"
    });
}

/// <summary>
/// Base type for issued certificate payloads.
/// </summary>
public abstract record CertificateRepresentation(CertificateFormat Format, string MediaType);

public sealed record JsonCertificateRepresentation(
    string Json,
    string CanonicalForm,
    SignatureProof Signature,
    string ProtocolVersion,
    string EntryHash
) : CertificateRepresentation(CertificateFormat.Json, "application/lockb0x+json");

public sealed record VerifiableCredentialRepresentation(
    string Credential,
    string CanonicalForm,
    SignatureProof Proof,
    IReadOnlyCollection<string> Contexts,
    string EntryHash,
    string Jws
) : CertificateRepresentation(CertificateFormat.VerifiableCredential, "application/ld+json");

public sealed record JwtCertificateRepresentation(
    string Token,
    string Header,
    string Payload,
    SignatureProof Signature,
    string EntryHash
) : CertificateRepresentation(CertificateFormat.Jwt, "application/jwt");

public sealed record CwtCertificateRepresentation(
    byte[] Payload,
    SignatureProof Signature,
    string EntryHash
) : CertificateRepresentation(CertificateFormat.Cwt, "application/cwt");

public sealed record X509CertificateRepresentation(
    byte[] Certificate,
    string EntryHashAlgorithm,
    byte[] EntryHash
) : CertificateRepresentation(CertificateFormat.X509, "application/pkix-cert");

/// <summary>
/// Represents the result of validating a certificate against a Codex entry.
/// </summary>
public sealed class CertificateValidationResult
{
    private readonly List<string> _errors = new();
    private readonly List<string> _warnings = new();

    public bool Success => _errors.Count == 0;

    public IReadOnlyList<string> Errors => _errors;

    public IReadOnlyList<string> Warnings => _warnings;

    public CertificateValidationResult AddError(string message)
    {
        _errors.Add(message);
        return this;
    }

    public CertificateValidationResult AddWarning(string message)
    {
        _warnings.Add(message);
        return this;
    }
}
