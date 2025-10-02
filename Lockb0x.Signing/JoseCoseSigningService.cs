using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Lockb0x.Core.Models;

namespace Lockb0x.Signing;

internal static class SigningAlgorithms
{
    public const string EdDsa = "EdDSA";
    public const string Es256K = "ES256K";
    public const string Rs256 = "RS256";

    public static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Algorithm must be provided", nameof(value));
        }

        var upper = value.Trim().ToUpperInvariant();
        return upper switch
        {
            "EDDSA" or "ED25519" => EdDsa,
            "ES256K" or "SECP256K1" => Es256K,
            "RS256" or "RSA" or "RSASSA-PKCS1-V1_5" => Rs256,
            _ => throw new NotSupportedException($"Unsupported signature algorithm '{value}'.")
        };
    }
}

/// <summary>
/// Provides JOSE/COSE compatible signing functionality for Codex Entries.
/// </summary>
public sealed class JoseCoseSigningService : ISigningService
{
    private readonly IKeyStore _keyStore;

    public JoseCoseSigningService()
        : this(new InMemoryKeyStore())
    {
    }

    public JoseCoseSigningService(IKeyStore keyStore)
    {
        _keyStore = keyStore ?? throw new ArgumentNullException(nameof(keyStore));
    }

    public async Task<SignatureProof> SignAsync(byte[] canonicalPayload, SigningKey key, string algorithm)
    {
        ArgumentNullException.ThrowIfNull(canonicalPayload);
        ArgumentNullException.ThrowIfNull(key);
        if (canonicalPayload.Length == 0)
        {
            throw new ArgumentException("Canonical payload cannot be empty.", nameof(canonicalPayload));
        }

        var normalizedAlgorithm = SigningAlgorithms.Normalize(algorithm);
        EnsureKeyMatchesAlgorithm(key, normalizedAlgorithm);

        if (string.IsNullOrWhiteSpace(key.PrivateKey))
        {
            throw new InvalidOperationException($"Signing key '{key.KeyId}' does not contain a private key.");
        }

        if (key.Revoked)
        {
            throw new InvalidOperationException($"Signing key '{key.KeyId}' has been revoked and cannot be used for signing.");
        }

        byte[] signature = normalizedAlgorithm switch
        {
            SigningAlgorithms.EdDsa => SignEd25519(canonicalPayload, key.PrivateKey!),
            SigningAlgorithms.Es256K => SignEs256K(canonicalPayload, key.PrivateKey!, key.PublicKey),
            SigningAlgorithms.Rs256 => SignRs256(canonicalPayload, key.PrivateKey!),
            _ => throw new NotSupportedException($"Unsupported signature algorithm '{normalizedAlgorithm}'.")
        };

        await _keyStore.AddKeyAsync(CloneKeyWithoutPrivateMaterial(key)).ConfigureAwait(false);

        return new SignatureProof
        {
            ProtectedHeader = new SignatureProtectedHeader
            {
                Algorithm = normalizedAlgorithm,
                KeyId = key.KeyId
            },
            Signature = Base64Url.Encode(signature)
        };
    }

    public async Task<bool> VerifyAsync(byte[] canonicalPayload, SignatureProof signature)
    {
        ArgumentNullException.ThrowIfNull(canonicalPayload);
        ArgumentNullException.ThrowIfNull(signature);
        ArgumentNullException.ThrowIfNull(signature.ProtectedHeader);

        if (canonicalPayload.Length == 0)
        {
            return false;
        }

        var normalizedAlgorithm = SigningAlgorithms.Normalize(signature.ProtectedHeader.Algorithm);
        var keyId = signature.ProtectedHeader.KeyId;
        if (string.IsNullOrWhiteSpace(keyId))
        {
            return false;
        }

        var key = await _keyStore.GetKeyAsync(keyId).ConfigureAwait(false);
        if (key is null || key.Revoked)
        {
            return false;
        }

        EnsureKeyMatchesAlgorithm(key, normalizedAlgorithm);
        if (string.IsNullOrWhiteSpace(key.PublicKey))
        {
            return false;
        }

        var signatureBytes = Base64Url.Decode(signature.Signature);

        return normalizedAlgorithm switch
        {
            SigningAlgorithms.EdDsa => VerifyEd25519(canonicalPayload, signatureBytes, key.PublicKey),
            SigningAlgorithms.Es256K => VerifyEs256K(canonicalPayload, signatureBytes, key.PublicKey),
            SigningAlgorithms.Rs256 => VerifyRs256(canonicalPayload, signatureBytes, key.PublicKey),
            _ => false
        };
    }

    public async Task<IList<SignatureProof>> MultiSignAsync(byte[] canonicalPayload, IEnumerable<SigningKey> keys, MultiSigPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(canonicalPayload);
        ArgumentNullException.ThrowIfNull(keys);
        ArgumentNullException.ThrowIfNull(policy);

        var keyList = keys.ToList();
        if (policy.Threshold < 1)
        {
            throw new ArgumentException("Multi-signature policies must require at least one signature.", nameof(policy));
        }

        if (keyList.Count == 0)
        {
            throw new InvalidOperationException("At least one signing key must be provided for multi-signature operations.");
        }

        var allowedIds = policy.AllowedKeyIds?.Count > 0
            ? new HashSet<string>(policy.AllowedKeyIds, StringComparer.Ordinal)
            : null;

        var signatures = new List<SignatureProof>();
        foreach (var key in keyList)
        {
            if (allowedIds is not null && !string.IsNullOrWhiteSpace(key.KeyId) && !allowedIds.Contains(key.KeyId))
            {
                continue;
            }

            signatures.Add(await SignAsync(canonicalPayload, key, key.Type).ConfigureAwait(false));
            if (signatures.Count >= policy.Threshold)
            {
                break;
            }
        }

        if (signatures.Count < policy.Threshold)
        {
            throw new InvalidOperationException($"Multi-signature policy requires {policy.Threshold} valid signature(s) but only {signatures.Count} could be produced.");
        }

        return signatures;
    }

    public async Task<bool> VerifyMultiSigAsync(byte[] canonicalPayload, IEnumerable<SignatureProof> signatures, MultiSigPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(canonicalPayload);
        ArgumentNullException.ThrowIfNull(signatures);
        ArgumentNullException.ThrowIfNull(policy);

        var proofs = signatures.ToList();
        if (proofs.Count < policy.Threshold)
        {
            return false;
        }

        var allowedIds = policy.AllowedKeyIds?.Count > 0
            ? new HashSet<string>(policy.AllowedKeyIds, StringComparer.Ordinal)
            : null;

        var verified = 0;
        var usedKeys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var proof in proofs)
        {
            if (proof?.ProtectedHeader is null)
            {
                continue;
            }

            var keyId = proof.ProtectedHeader.KeyId;
            if (string.IsNullOrWhiteSpace(keyId))
            {
                continue;
            }

            if (allowedIds is not null && !allowedIds.Contains(keyId))
            {
                continue;
            }

            if (!usedKeys.Add(keyId))
            {
                continue;
            }

            if (await VerifyAsync(canonicalPayload, proof).ConfigureAwait(false))
            {
                verified++;
                if (verified >= policy.Threshold)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static SigningKey CloneKeyWithoutPrivateMaterial(SigningKey key)
    {
        return new SigningKey
        {
            KeyId = key.KeyId,
            Type = key.Type,
            PublicKey = key.PublicKey,
            Controller = key.Controller,
            Revoked = key.Revoked,
            RevokedAt = key.RevokedAt
        };
    }

    private static void EnsureKeyMatchesAlgorithm(SigningKey key, string algorithm)
    {
        if (string.IsNullOrWhiteSpace(key.Type))
        {
            throw new InvalidOperationException($"Signing key '{key.KeyId}' is missing a key type.");
        }

        var normalized = SigningAlgorithms.Normalize(key.Type);
        if (!string.Equals(normalized, algorithm, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Signing key '{key.KeyId}' is configured for '{normalized}' but the '{algorithm}' algorithm was requested.");
        }
    }

    private static byte[] SignEd25519(ReadOnlySpan<byte> payload, string privateKeyMaterial)
    {
        var privateKey = DecodeKeyMaterial(privateKeyMaterial);
        if (privateKey.Length != 32)
        {
            throw new InvalidOperationException("Ed25519 private keys must be 32 bytes long (seed).");
        }

        var key = NSec.Cryptography.Key.Import(
            NSec.Cryptography.SignatureAlgorithm.Ed25519,
            privateKey,
            NSec.Cryptography.KeyBlobFormat.RawPrivateKey,
            new NSec.Cryptography.KeyCreationParameters { ExportPolicy = NSec.Cryptography.KeyExportPolicies.AllowPlaintextExport }
        );
        return NSec.Cryptography.SignatureAlgorithm.Ed25519.Sign(key, payload.ToArray());
    }

    private static byte[] SignEs256K(ReadOnlySpan<byte> payload, string privateKeyMaterial, string publicKeyMaterial)
    {
        using var ecdsa = CreateSecp256K1(privateKeyMaterial, publicKeyMaterial, requirePrivate: true);
        var derSignature = ecdsa.SignData(payload, HashAlgorithmName.SHA256);
        return DerToIeeeP1363(derSignature, 64);
    }

    private static byte[] SignRs256(ReadOnlySpan<byte> payload, string privateKeyMaterial)
    {
        using var rsa = CreateRsa(privateKeyMaterial, includePrivate: true);
        return rsa.SignData(payload, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }

    private static bool VerifyEd25519(ReadOnlySpan<byte> payload, ReadOnlySpan<byte> signature, string publicKeyMaterial)
    {
        var publicKey = DecodeKeyMaterial(publicKeyMaterial);
        if (publicKey.Length != 32)
        {
            throw new InvalidOperationException("Ed25519 public keys must be 32 bytes long.");
        }

        var pubKey = NSec.Cryptography.PublicKey.Import(NSec.Cryptography.SignatureAlgorithm.Ed25519, publicKey, NSec.Cryptography.KeyBlobFormat.RawPublicKey);
        return NSec.Cryptography.SignatureAlgorithm.Ed25519.Verify(pubKey, payload.ToArray(), signature.ToArray());
    }

    private static bool VerifyEs256K(ReadOnlySpan<byte> payload, ReadOnlySpan<byte> signature, string publicKeyMaterial)
    {
        using var ecdsa = CreateSecp256K1(null, publicKeyMaterial, requirePrivate: false);
        var derSignature = IeeeP1363ToDer(signature, 64);
        return ecdsa.VerifyData(payload, derSignature, HashAlgorithmName.SHA256);
    }

    private static bool VerifyRs256(ReadOnlySpan<byte> payload, ReadOnlySpan<byte> signature, string publicKeyMaterial)
    {
        using var rsa = CreateRsa(publicKeyMaterial, includePrivate: false);
        return rsa.VerifyData(payload, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }

    private static ECDsa CreateSecp256K1(string? privateKeyMaterial, string publicKeyMaterial, bool requirePrivate)
    {
        var ecdsa = ECDsa.Create();
        if (requirePrivate)
        {
            if (string.IsNullOrWhiteSpace(privateKeyMaterial))
            {
                throw new InvalidOperationException("An EC private key is required for signing operations.");
            }

            if (TryImportPem(ecdsa, privateKeyMaterial))
            {
                return ecdsa;
            }

            var privateKeyBytes = DecodeKeyMaterial(privateKeyMaterial);
            try
            {
                ecdsa.ImportPkcs8PrivateKey(privateKeyBytes, out _);
            }
            catch (CryptographicException)
            {
                try
                {
                    ecdsa.ImportECPrivateKey(privateKeyBytes, out _);
                }
                catch (CryptographicException ex)
                {
                    throw new InvalidOperationException("Unable to import secp256k1 private key material.", ex);
                }
            }

            return ecdsa;
        }

        if (TryImportPem(ecdsa, publicKeyMaterial))
        {
            return ecdsa;
        }

        var publicKeyBytes = DecodeKeyMaterial(publicKeyMaterial);
        try
        {
            ecdsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);
        }
        catch (CryptographicException)
        {
            var point = ParseSecp256K1PublicPoint(publicKeyBytes);
            ecdsa.ImportParameters(new ECParameters
            {
                Curve = ECCurve.CreateFromFriendlyName("secP256k1"),
                Q = point
            });
        }

        return ecdsa;
    }

    private static RSA CreateRsa(string keyMaterial, bool includePrivate)
    {
        var rsa = RSA.Create();
        if (TryImportPem(rsa, keyMaterial))
        {
            return rsa;
        }

        var bytes = DecodeKeyMaterial(keyMaterial);
        if (includePrivate)
        {
            try
            {
                rsa.ImportPkcs8PrivateKey(bytes, out _);
            }
            catch (CryptographicException)
            {
                try
                {
                    rsa.ImportRSAPrivateKey(bytes, out _);
                }
                catch (CryptographicException ex)
                {
                    throw new InvalidOperationException("Unable to import RSA private key material.", ex);
                }
            }
        }
        else
        {
            try
            {
                rsa.ImportSubjectPublicKeyInfo(bytes, out _);
            }
            catch (CryptographicException)
            {
                try
                {
                    rsa.ImportRSAPublicKey(bytes, out _);
                }
                catch (CryptographicException ex)
                {
                    throw new InvalidOperationException("Unable to import RSA public key material.", ex);
                }
            }
        }

        return rsa;
    }

    private static bool TryImportPem(AsymmetricAlgorithm algorithm, string material)
    {
        if (!material.Contains("-----BEGIN", StringComparison.Ordinal))
        {
            return false;
        }

        try
        {
            algorithm.ImportFromPem(material);
            return true;
        }
        catch (CryptographicException)
        {
            return false;
        }
    }

    private static byte[] DerToIeeeP1363(ReadOnlySpan<byte> der, int size)
    {
        var reader = new AsnReader(new ReadOnlyMemory<byte>(der.ToArray()), AsnEncodingRules.DER);
        var sequence = reader.ReadSequence();
        var r = sequence.ReadIntegerBytes();
        var s = sequence.ReadIntegerBytes();
        reader.ThrowIfNotEmpty();

        var buffer = new byte[size];
        CopyInteger(r.Span, buffer.AsSpan(0, size / 2));
        CopyInteger(s.Span, buffer.AsSpan(size / 2));
        return buffer;
    }

    private static byte[] IeeeP1363ToDer(ReadOnlySpan<byte> signature, int size)
    {
        if (signature.Length != size)
        {
            throw new InvalidOperationException($"Expected a {size}-byte raw ECDSA signature but received {signature.Length} bytes.");
        }

        var writer = new AsnWriter(AsnEncodingRules.DER);
        writer.PushSequence();
        writer.WriteInteger(signature[..(size / 2)]);
        writer.WriteInteger(signature[(size / 2)..]);
        writer.PopSequence();
        return writer.Encode();
    }

    private static void CopyInteger(ReadOnlySpan<byte> source, Span<byte> destination)
    {
        destination.Clear();
        if (source.Length == 0)
        {
            return;
        }

        if (source[0] == 0x00)
        {
            source = source[1..];
        }

        if (source.Length > destination.Length)
        {
            throw new InvalidOperationException("Integer component is longer than the expected field length.");
        }

        source.CopyTo(destination[(destination.Length - source.Length)..]);
    }

    private static ECPoint ParseSecp256K1PublicPoint(ReadOnlySpan<byte> keyBytes)
    {
        if (keyBytes.Length == 65 && keyBytes[0] == 0x04)
        {
            return new ECPoint
            {
                X = keyBytes.Slice(1, 32).ToArray(),
                Y = keyBytes.Slice(33, 32).ToArray()
            };
        }

        if (keyBytes.Length == 64)
        {
            return new ECPoint
            {
                X = keyBytes[..32].ToArray(),
                Y = keyBytes[32..].ToArray()
            };
        }

        throw new InvalidOperationException("Unsupported secp256k1 public key encoding. Provide an uncompressed point or SubjectPublicKeyInfo.");
    }

    private static byte[] DecodeKeyMaterial(string material)
    {
        if (string.IsNullOrWhiteSpace(material))
        {
            throw new ArgumentException("Key material cannot be empty.", nameof(material));
        }

        material = material.Trim();

        if (material.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            return Convert.FromHexString(material[2..]);
        }

        if (IsLikelyHex(material))
        {
            return Convert.FromHexString(material);
        }

        try
        {
            return Base64Url.Decode(material);
        }
        catch (FormatException)
        {
            return Convert.FromBase64String(material);
        }
    }

    private static bool IsLikelyHex(string value)
    {
        if (value.Length % 2 != 0)
        {
            return false;
        }

        foreach (var c in value)
        {
            if (!Uri.IsHexDigit(c))
            {
                return false;
            }
        }

        return true;
    }
}

internal static class Base64Url
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
            throw new ArgumentException("Value cannot be null or empty.", nameof(value));
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

        switch (builder.Length % 4)
        {
            case 2:
                builder.Append("==");
                break;
            case 3:
                builder.Append('=');
                break;
        }

        return Convert.FromBase64String(builder.ToString());
    }
}

public class InMemoryKeyStore : IKeyStore
{
    private readonly ConcurrentDictionary<string, SigningKey> _keys = new(StringComparer.Ordinal);

    public Task<SigningKey?> GetKeyAsync(string keyId)
    {
        if (string.IsNullOrWhiteSpace(keyId))
        {
            return Task.FromResult<SigningKey?>(null);
        }

        _keys.TryGetValue(keyId, out var key);
        return Task.FromResult(key);
    }

    public Task<IEnumerable<SigningKey>> ListKeysAsync()
    {
        return Task.FromResult<IEnumerable<SigningKey>>(_keys.Values.Select(Clone).ToList());
    }

    public Task AddKeyAsync(SigningKey key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (string.IsNullOrWhiteSpace(key.KeyId))
        {
            throw new ArgumentException("Signing keys must have a non-empty identifier.", nameof(key));
        }

        _keys[key.KeyId] = Clone(key);
        return Task.CompletedTask;
    }

    public Task RevokeKeyAsync(string keyId)
    {
        if (_keys.TryGetValue(keyId, out var key))
        {
            key.Revoked = true;
            key.RevokedAt = DateTimeOffset.UtcNow;
        }

        return Task.CompletedTask;
    }

    public Task<bool> IsRevokedAsync(string keyId)
    {
        var revoked = _keys.TryGetValue(keyId, out var key) && key.Revoked;
        return Task.FromResult(revoked);
    }

    private static SigningKey Clone(SigningKey key)
    {
        return new SigningKey
        {
            KeyId = key.KeyId,
            Type = key.Type,
            PublicKey = key.PublicKey,
            PrivateKey = key.PrivateKey,
            Controller = key.Controller,
            Revoked = key.Revoked,
            RevokedAt = key.RevokedAt
        };
    }
}
