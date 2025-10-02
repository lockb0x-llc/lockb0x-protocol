using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Lockb0x.Signing;
using Xunit;

namespace Lockb0x.Tests;

public class SigningServiceTests
{
    private static readonly byte[] SamplePayload = Encoding.UTF8.GetBytes("{\"id\":\"urn:uuid:test\"}");

    [Fact]
    public async Task SignAndVerify_Ed25519_RoundTrip()
    {
        var service = new JoseCoseSigningService();
        var key = CreateEd25519Key("did:example:alice", "k-ed25519");

        var proof = await service.SignAsync(SamplePayload, key, "EdDSA");

        Assert.Equal("EdDSA", proof.ProtectedHeader.Algorithm);
        Assert.Equal("k-ed25519", proof.ProtectedHeader.KeyId);
        Assert.True(await service.VerifyAsync(SamplePayload, proof));
    }

    [Fact]
    public async Task SignAndVerify_Secp256k1_RoundTrip()
    {
        using var ecdsa = ECDsa.Create(ECCurve.CreateFromFriendlyName("secP256k1"));
        var key = new SigningKey
        {
            KeyId = "k-secp256k1",
            Type = "ES256K",
            PrivateKey = Convert.ToBase64String(ecdsa.ExportPkcs8PrivateKey()),
            PublicKey = Convert.ToBase64String(ecdsa.ExportSubjectPublicKeyInfo())
        };

        var service = new JoseCoseSigningService();
        var proof = await service.SignAsync(SamplePayload, key, "ES256K");

        Assert.Equal("ES256K", proof.ProtectedHeader.Algorithm);
        var signatureBytes = DecodeBase64Url(proof.Signature);
        Assert.Equal(64, signatureBytes.Length);
        Assert.True(await service.VerifyAsync(SamplePayload, proof));
    }

    [Fact]
    public async Task SignAndVerify_Rs256_RoundTrip()
    {
        using var rsa = RSA.Create(2048);
        var key = new SigningKey
        {
            KeyId = "k-rs256",
            Type = "RS256",
            PrivateKey = Convert.ToBase64String(rsa.ExportPkcs8PrivateKey()),
            PublicKey = Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo())
        };

        var service = new JoseCoseSigningService();
        var proof = await service.SignAsync(SamplePayload, key, "RS256");

        Assert.Equal("RS256", proof.ProtectedHeader.Algorithm);
        Assert.True(await service.VerifyAsync(SamplePayload, proof));
    }

    [Fact]
    public async Task Verify_ReturnsFalse_WhenKeyRevoked()
    {
        var store = new InMemoryKeyStore();
        var service = new JoseCoseSigningService(store);
        var key = CreateEd25519Key("did:example:bob", "revoked-key");

        await store.AddKeyAsync(new SigningKey
        {
            KeyId = key.KeyId,
            Type = key.Type,
            PublicKey = key.PublicKey
        });

        var proof = await service.SignAsync(SamplePayload, key, key.Type);
        await store.RevokeKeyAsync(key.KeyId);

        Assert.False(await service.VerifyAsync(SamplePayload, proof));
    }

    [Fact]
    public async Task MultiSignAndVerify_Succeeds_WithThreshold()
    {
        var service = new JoseCoseSigningService();
        var key1 = CreateEd25519Key("did:example:alice", "multi-1");
        var key2 = CreateEd25519Key("did:example:bob", "multi-2");
        var policy = new MultiSigPolicy
        {
            Threshold = 2,
            AllowedKeyIds = new List<string> { key1.KeyId, key2.KeyId }
        };

        var signatures = await service.MultiSignAsync(SamplePayload, new[] { key1, key2 }, policy);

        Assert.Equal(2, signatures.Count);
        Assert.True(await service.VerifyMultiSigAsync(SamplePayload, signatures, policy));
        Assert.False(await service.VerifyMultiSigAsync(SamplePayload, signatures.Take(1), policy));
    }

    [Fact]
    public async Task MultiSignAsync_Throws_WhenInsufficientAllowedKeys()
    {
        var service = new JoseCoseSigningService();
        var key1 = CreateEd25519Key("did:example:alice", "multi-3");
        var key2 = CreateEd25519Key("did:example:bob", "multi-4");
        var policy = new MultiSigPolicy
        {
            Threshold = 2,
            AllowedKeyIds = new List<string> { key1.KeyId }
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.MultiSignAsync(SamplePayload, new[] { key1, key2 }, policy));
    }

    [Fact]
    public async Task SignAsync_Throws_WhenKeyTypeMismatch()
    {
        var service = new JoseCoseSigningService();
        var key = CreateEd25519Key("did:example:carol", "mismatch");

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SignAsync(SamplePayload, key, "RS256"));
    }

    [Fact]
    public async Task VerifyMultiSig_ReturnsFalse_WhenDuplicateSigner()
    {
        var service = new JoseCoseSigningService();
        var key = CreateEd25519Key("did:example:dan", "dup");
        var policy = new MultiSigPolicy { Threshold = 1, AllowedKeyIds = new List<string> { key.KeyId } };

        var signature = await service.SignAsync(SamplePayload, key, key.Type);
        var proofs = new[] { signature, signature };

        Assert.True(await service.VerifyMultiSigAsync(SamplePayload, proofs, policy));

        policy.Threshold = 2;
        Assert.False(await service.VerifyMultiSigAsync(SamplePayload, proofs, policy));
    }

    private static SigningKey CreateEd25519Key(string controller, string keyId)
    {
        // Use a valid 32-byte Ed25519 seed for the private key (RFC 8032 test vector)
        var privateKeyHex = "9d61b19deffd5a60ba844af492ec2cc44449c5697b326919703bac031cae7f60"; // 32 bytes
        var publicKeyHex = "d75a980182b10ab7d54bfed3c964073a0ee172f3daa62325af021a68f707511a"; // 32 bytes

        return new SigningKey
        {
            KeyId = keyId,
            Type = "Ed25519",
            Controller = controller,
            PrivateKey = Convert.ToBase64String(Convert.FromHexString(privateKeyHex)),
            PublicKey = Convert.ToBase64String(Convert.FromHexString(publicKeyHex))
        };
    }

    private static byte[] DecodeBase64Url(string value)
    {
        var builder = new StringBuilder(value);
        builder.Replace('-', '+').Replace('_', '/');
        while (builder.Length % 4 != 0)
        {
            builder.Append('=');
        }

        return Convert.FromBase64String(builder.ToString());
    }
}
