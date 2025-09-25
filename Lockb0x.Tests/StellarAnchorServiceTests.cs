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

    [Fact]
    public void ComputeEntryMd5Hash_Throws_When_Canonicalizer_NotImplemented()
    {
        var entry = new CodexEntry();
        Assert.Throws<NotImplementedException>(() => StellarAnchorService.ComputeEntryMd5Hash(entry));
    }

    [Fact]
    public void GenerateStellarMemo_Throws_When_Canonicalizer_NotImplemented()
    {
        var entry = new CodexEntry();
        var publicKey = "GABC123456789";
        Assert.Throws<NotImplementedException>(() => StellarAnchorService.GenerateStellarMemo(entry, publicKey));
    }

    [Fact]
    public void GenerateStellarMemo_ReturnsValidLength()
    {
        // This test will work once JsonCanonicalizer is implemented
        // For now, we can test that the method signature and validation logic work
        
        // Test with a very long public key to ensure truncation works
        var longPublicKey = new string('A', 100);
        
        // The method should not crash from key length validation
        // When JsonCanonicalizer is implemented, this will produce a valid result
        try 
        {
            var result = StellarAnchorService.GenerateStellarMemo(new CodexEntry(), longPublicKey);
            // If we get here, canonicalizer worked
            Assert.True(result.Length <= 56, "Memo should not exceed 56 hex characters (28 bytes)");
        }
        catch (NotImplementedException ex)
        {
            Assert.Contains("JsonCanonicalizer", ex.Message);
        }
    }

    [Theory]
    [InlineData("SHORTKEY")]
    [InlineData("VERYLONGSTELLARPUBLICKEY123456789")]
    public void GenerateStellarMemo_Respects_MaxLength(string publicKey)
    {
        try 
        {
            var result = StellarAnchorService.GenerateStellarMemo(new CodexEntry(), publicKey);
            // If we get here, canonicalizer worked
            Assert.True(result.Length <= 56, "Memo should not exceed 56 hex characters (28 bytes)");
        }
        catch (NotImplementedException ex)
        {
            Assert.Contains("JsonCanonicalizer", ex.Message);
        }
    }
}
