using Lockb0x.Signing;
using Xunit;
using System;
using System.Threading.Tasks;

namespace Lockb0x.Tests;

public class SigningServiceTests
{
    [Fact]
    public async Task JoseCoseSigningService_Throws_NotImplemented()
    {
        var service = new JoseCoseSigningService();
        var key = new SigningKey { KeyId = "test", Type = "Ed25519", PublicKey = "pub" };
        await Assert.ThrowsAsync<NotImplementedException>(() => service.SignAsync(new byte[] { 1 }, key, "Ed25519"));
        await Assert.ThrowsAsync<NotImplementedException>(() => service.VerifyAsync(new byte[] { 1 }, null!));
    }

    [Fact]
    public async Task InMemoryKeyStore_BasicOps()
    {
        var store = new InMemoryKeyStore();
        var key = new SigningKey { KeyId = "k1", Type = "Ed25519", PublicKey = "pub" };
        await store.AddKeyAsync(key);
        var fetched = await store.GetKeyAsync("k1");
        Assert.NotNull(fetched);
        Assert.Equal("k1", fetched!.KeyId);
        await store.RevokeKeyAsync("k1");
        var revoked = await store.GetKeyAsync("k1");
        Assert.True(revoked!.Revoked);
        var keys = await store.ListKeysAsync();
        Assert.Contains(keys, k => k.KeyId == "k1");
    }
}
