using System.Threading.Tasks;
using Lockb0x.Core;

namespace Lockb0x.Signing;

public interface ISigningService
{
    Task<SignatureProof> SignAsync(byte[] canonicalPayload, SigningKey key, string algorithm);
    Task<bool> VerifyAsync(byte[] canonicalPayload, SignatureProof signature);
}

public class SigningKey
{
    public string KeyId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // e.g., Ed25519, secp256k1, RSA
    public string PublicKey { get; set; } = string.Empty;
    public string? PrivateKey { get; set; } // Should be securely managed
    public string? Controller { get; set; } // e.g., DID or Stellar account
    public bool Revoked { get; set; } = false;
}

// JOSE/COSE signing service stub
public class JoseCoseSigningService : ISigningService
{
    public Task<SignatureProof> SignAsync(byte[] canonicalPayload, SigningKey key, string algorithm) => throw new System.NotImplementedException();
    public Task<bool> VerifyAsync(byte[] canonicalPayload, SignatureProof signature) => throw new System.NotImplementedException();
}

// Key store stub
public interface IKeyStore
{
    Task<SigningKey?> GetKeyAsync(string keyId);
    Task<IEnumerable<SigningKey>> ListKeysAsync();
    Task AddKeyAsync(SigningKey key);
    Task RevokeKeyAsync(string keyId);
}

public class InMemoryKeyStore : IKeyStore
{
    private readonly Dictionary<string, SigningKey> _keys = new();
    public Task<SigningKey?> GetKeyAsync(string keyId) => Task.FromResult(_keys.TryGetValue(keyId, out var key) ? key : null);
    public Task<IEnumerable<SigningKey>> ListKeysAsync() => Task.FromResult<IEnumerable<SigningKey>>(_keys.Values);
    public Task AddKeyAsync(SigningKey key) { _keys[key.KeyId] = key; return Task.CompletedTask; }
    public Task RevokeKeyAsync(string keyId) { if (_keys.TryGetValue(keyId, out var key)) key.Revoked = true; return Task.CompletedTask; }
}
