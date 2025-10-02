using System.Collections.Generic;
using System.Threading.Tasks;
using Lockb0x.Core.Models;

namespace Lockb0x.Signing;

public interface ISigningService
{
    Task<SignatureProof> SignAsync(byte[] canonicalPayload, SigningKey key, string algorithm);
    Task<bool> VerifyAsync(byte[] canonicalPayload, SignatureProof signature);
    Task<IList<SignatureProof>> MultiSignAsync(byte[] canonicalPayload, IEnumerable<SigningKey> keys, MultiSigPolicy policy);
    Task<bool> VerifyMultiSigAsync(byte[] canonicalPayload, IEnumerable<SignatureProof> signatures, MultiSigPolicy policy);
}

public class SigningKey
{
    public string KeyId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // e.g., Ed25519, secp256k1, RSA
    public string PublicKey { get; set; } = string.Empty;
    public string? PrivateKey { get; set; } // Should be securely managed
    public string? Controller { get; set; } // e.g., DID or Stellar account
    public bool Revoked { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
}

public class MultiSigPolicy
{
    public int Threshold { get; set; } = 1; // Number of required signatures
    public List<string> AllowedKeyIds { get; set; } = new();
}

public interface IKeyStore
{
    Task<SigningKey?> GetKeyAsync(string keyId);
    Task<IEnumerable<SigningKey>> ListKeysAsync();
    Task AddKeyAsync(SigningKey key);
    Task RevokeKeyAsync(string keyId);
    Task<bool> IsRevokedAsync(string keyId);
}
