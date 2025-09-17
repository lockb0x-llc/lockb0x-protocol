using System.Threading.Tasks;
using Lockb0x.Core;

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
    public bool Revoked { get; set; } = false;
    public DateTimeOffset? RevokedAt { get; set; }
}

public class MultiSigPolicy
{
    public int Threshold { get; set; } = 1; // Number of required signatures
    public List<string> AllowedKeyIds { get; set; } = new();
}


// JOSE/COSE signing service stub with multi-sig and algorithm selection
public class JoseCoseSigningService : ISigningService
{
    public Task<SignatureProof> SignAsync(byte[] canonicalPayload, SigningKey key, string algorithm)
    {
        // TODO: Implement Ed25519, secp256k1, RSA signing using JOSE/COSE
        throw new NotImplementedException();
    }

    public Task<bool> VerifyAsync(byte[] canonicalPayload, SignatureProof signature)
    {
        // TODO: Implement signature verification for supported algorithms
        throw new NotImplementedException();
    }

    public Task<IList<SignatureProof>> MultiSignAsync(byte[] canonicalPayload, IEnumerable<SigningKey> keys, MultiSigPolicy policy)
    {
        // TODO: Implement multi-sig signing logic
        throw new NotImplementedException();
    }

    public Task<bool> VerifyMultiSigAsync(byte[] canonicalPayload, IEnumerable<SignatureProof> signatures, MultiSigPolicy policy)
    {
        // TODO: Implement multi-sig verification logic
        throw new NotImplementedException();
    }
}

// Key store stub

public interface IKeyStore
{
    Task<SigningKey?> GetKeyAsync(string keyId);
    Task<IEnumerable<SigningKey>> ListKeysAsync();
    Task AddKeyAsync(SigningKey key);
    Task RevokeKeyAsync(string keyId);
    Task<bool> IsRevokedAsync(string keyId);
}


public class InMemoryKeyStore : IKeyStore
{
    private readonly Dictionary<string, SigningKey> _keys = new();
    public Task<SigningKey?> GetKeyAsync(string keyId) => Task.FromResult(_keys.TryGetValue(keyId, out var key) ? key : null);
    public Task<IEnumerable<SigningKey>> ListKeysAsync() => Task.FromResult<IEnumerable<SigningKey>>(_keys.Values);
    public Task AddKeyAsync(SigningKey key) { _keys[key.KeyId] = key; return Task.CompletedTask; }
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
        return Task.FromResult(_keys.TryGetValue(keyId, out var key) && key.Revoked);
    }
}
