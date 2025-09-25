using Lockb0x.Core;
using Xunit;
using System;
using System.Text.Json;
using System.Collections.Generic;

namespace Lockb0x.Tests;

public class CodexEntryTests
{
    [Fact]
    public void CodexEntry_Serializes_And_Deserializes()
    {
        var entry = new CodexEntry
        {
            Id = "urn:uuid:1234",
            Timestamp = DateTimeOffset.UtcNow,
            Storage = new List<StorageProof> { new StorageProof { Location = "ni:///sha-256;abc" } },
            Integrity = new List<IntegrityProof> { new IntegrityProof { Algorithm = "sha-256", Hash = "abc" } },
            Signatures = new List<SignatureProof> { new SignatureProof { Algorithm = "Ed25519", Value = "sig", Signer = "did:example:alice" } },
            Anchors = new List<AnchorProof> { new AnchorProof { ChainId = "stellar:testnet", TxHash = "tx123", HashAlgorithm = "sha-256" } },
            Provenance = new ProvenanceMetadata { WasGeneratedBy = "did:example:alice" },
            Revision = null
        };
        var json = JsonSerializer.Serialize(entry);
        var deserialized = JsonSerializer.Deserialize<CodexEntry>(json);
        Assert.NotNull(deserialized);
        Assert.Equal(entry.Id, deserialized!.Id);
        Assert.Equal(entry.Storage[0].Location, deserialized.Storage[0].Location);
        Assert.Equal(entry.Integrity[0].Hash, deserialized.Integrity[0].Hash);
        Assert.Equal(entry.Signatures[0].Value, deserialized.Signatures[0].Value);
        Assert.Equal(entry.Anchors[0].TxHash, deserialized.Anchors[0].TxHash);
        Assert.Equal(entry.Provenance.WasGeneratedBy, deserialized.Provenance.WasGeneratedBy);
    }

    [Fact]
    public void JsonCanonicalizer_Throws_NotImplemented()
    {
        Assert.Throws<NotImplementedException>(() => JsonCanonicalizer.Canonicalize(new object()));
    }

    [Fact]
    public void NiUriHelper_Throws_NotImplemented()
    {
        Assert.Throws<NotImplementedException>(() => NiUriHelper.ComputeNiUri(new byte[] { 1, 2, 3 }));
    }

    [Fact]
    public void CodexEntryValidator_Returns_False()
    {
        var entry = new CodexEntry();
        var result = CodexEntryValidator.Validate(entry, out var errors);
        Assert.False(result);
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void ValidateStellarAnchor_RejectsNullAnchor()
    {
        var result = CodexEntryValidator.ValidateStellarAnchor(null!, out var errors);
        Assert.False(result);
        Assert.Contains("AnchorProof cannot be null", errors);
    }

    [Fact]
    public void ValidateStellarAnchor_ValidatesStellarChain()
    {
        var anchor = new AnchorProof
        {
            ChainId = "stellar:testnet",
            TxHash = "abc123",
            HashAlgorithm = "sha-256"
        };
        
        var result = CodexEntryValidator.ValidateStellarAnchor(anchor, out var errors);
        Assert.True(result);
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateStellarAnchor_RejectsMissingHashAlgorithm()
    {
        var anchor = new AnchorProof
        {
            ChainId = "stellar:testnet",
            TxHash = "abc123",
            HashAlgorithm = ""
        };
        
        var result = CodexEntryValidator.ValidateStellarAnchor(anchor, out var errors);
        Assert.False(result);
        Assert.Contains("Stellar anchors must specify a hash_alg field", errors);
    }

    [Fact]
    public void ValidateStellarAnchor_RejectsWrongHashAlgorithm()
    {
        var anchor = new AnchorProof
        {
            ChainId = "stellar:testnet",
            TxHash = "abc123",
            HashAlgorithm = "md5"
        };
        
        var result = CodexEntryValidator.ValidateStellarAnchor(anchor, out var errors);
        Assert.False(result);
        Assert.Contains("Stellar anchors must use SHA-256 hash algorithm, but got 'md5'", errors);
    }

    [Fact]
    public void ValidateStellarAnchor_RejectsMissingTxHash()
    {
        var anchor = new AnchorProof
        {
            ChainId = "stellar:testnet",
            TxHash = "",
            HashAlgorithm = "sha-256"
        };
        
        var result = CodexEntryValidator.ValidateStellarAnchor(anchor, out var errors);
        Assert.False(result);
        Assert.Contains("Stellar anchors must have a valid tx_hash", errors);
    }

    [Fact]
    public void ValidateStellarAnchor_IgnoresNonStellarChains()
    {
        var anchor = new AnchorProof
        {
            ChainId = "ethereum:1",
            TxHash = "abc123",
            HashAlgorithm = "sha-256"
        };
        
        var result = CodexEntryValidator.ValidateStellarAnchor(anchor, out var errors);
        Assert.True(result);  // Non-Stellar chains pass validation
        Assert.Empty(errors);
    }
}
