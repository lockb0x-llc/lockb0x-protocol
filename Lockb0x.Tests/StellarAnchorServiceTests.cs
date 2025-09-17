using Lockb0x.Anchor.Stellar;
using Lockb0x.Core;
using Xunit;
using System;
using System.Threading.Tasks;

namespace Lockb0x.Tests;

public class StellarAnchorServiceTests
{
    [Fact]
    public async Task StellarAnchorService_Throws_NotImplemented()
    {
        var service = new StellarAnchorService();
        var entry = new CodexEntry();
        var anchor = new AnchorProof();
        // AnchorAsync should throw NotImplementedException due to JsonCanonicalizer
        var ex = await Assert.ThrowsAsync<NotImplementedException>(() => service.AnchorAsync(entry));
        Assert.Contains("JsonCanonicalizer", ex.Message);
        await Assert.ThrowsAsync<NotImplementedException>(() => service.VerifyAnchorAsync(anchor, entry, "pubnet"));
        await Assert.ThrowsAsync<NotImplementedException>(() => service.GetTransactionUrlAsync(anchor, "pubnet"));
    }

    [Fact]
    public void ComputeEntryHash_Throws_When_Canonicalizer_NotImplemented()
    {
        var entry = new CodexEntry();
        Assert.Throws<NotImplementedException>(() => StellarAnchorService.ComputeEntryHash(entry));
    }

    [Theory]
    [InlineData("testnet", "stellar:testnet")]
    [InlineData("pubnet", "stellar:pubnet")]
    [InlineData("customnet", "stellar:customnet")]
    [InlineData("TESTNET", "stellar:testnet")]
    public void GetCaip2ChainId_Maps_Networks_Correctly(string network, string expected)
    {
        var result = StellarAnchorService.GetCaip2ChainId(network);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("testnet", true)]
    [InlineData("dry-run", true)]
    [InlineData("pubnet", false)]
    [InlineData("mainnet", false)]
    public void IsDryRun_Returns_Expected(string network, bool expected)
    {
        var result = StellarAnchorService.IsDryRun(network);
        Assert.Equal(expected, result);
    }
}
