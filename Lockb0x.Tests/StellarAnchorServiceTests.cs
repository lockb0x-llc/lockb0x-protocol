using System;
using System.Threading.Tasks;
using Lockb0x.Anchor.Stellar;
using Lockb0x.Core.Canonicalization;
using Lockb0x.Core.Models;

namespace Lockb0x.Tests;

public class StellarAnchorServiceTests
{
    private const string DefaultPublicKey = "GCFXTESTACCOUNT000000000000000000000000000000000000000000000";

    [Fact]
    public async Task AnchorAsync_ReturnsAnchorProof_WithChainAndHash()
    {
        var service = CreateService(out var entry);
        var proof = await service.AnchorAsync(entry, "testnet", DefaultPublicKey);

        Assert.Equal("stellar:testnet", proof.Chain);
        Assert.Equal("sha-256", proof.HashAlgorithm);
        Assert.False(string.IsNullOrWhiteSpace(proof.TransactionHash));
        Assert.True(proof.AnchoredAt.HasValue);
    }

    [Fact]
    public async Task AnchorAsync_IsIdempotent_ForSameEntry()
    {
        var service = CreateService(out var entry);
        var first = await service.AnchorAsync(entry, "testnet", DefaultPublicKey);
        var second = await service.AnchorAsync(entry, "testnet", DefaultPublicKey);

        Assert.Equal(first.TransactionHash, second.TransactionHash);
    }

    [Fact]
    public async Task VerifyAnchorAsync_ReturnsTrue_WhenMemoMatches()
    {
        var service = CreateService(out var entry);
        var anchor = await service.AnchorAsync(entry, "testnet", DefaultPublicKey);

        var verified = await service.VerifyAnchorAsync(anchor, entry, "testnet", DefaultPublicKey);
        Assert.True(verified);
    }

    [Fact]
    public async Task VerifyAnchorAsync_ReturnsFalse_WhenEntryTampered()
    {
        var service = CreateService(out var entry);
        var anchor = await service.AnchorAsync(entry, "testnet", DefaultPublicKey);

        var tampered = CreateEntry(artifact: "TamperedArtifact");
        var verified = await service.VerifyAnchorAsync(anchor, tampered, "testnet", DefaultPublicKey);

        Assert.False(verified);
    }

    [Fact]
    public async Task GetTransactionUrlAsync_UsesExplorerTemplate()
    {
        var service = CreateService(out var entry);
        var anchor = await service.AnchorAsync(entry, "testnet", DefaultPublicKey);

        var url = await service.GetTransactionUrlAsync(anchor, "testnet");
        Assert.Contains(anchor.TransactionHash, url);
        Assert.Contains("stellar.expert", url);
    }

    private static StellarAnchorService CreateService(out CodexEntry entry)
    {
        var canonicalizer = new JcsCanonicalizer();
        var horizonClient = new InMemoryStellarHorizonClient();
        var options = StellarAnchorServiceOptions.CreateDefault();
        options.Networks["testnet"].DefaultAccountPublicKey = DefaultPublicKey;

        var service = new StellarAnchorService(canonicalizer, horizonClient, options: options);
        entry = CreateEntry();
        return service;
    }

    private static CodexEntry CreateEntry(string artifact = "SampleArtifact")
    {
        return new CodexEntryBuilder()
            .WithId(Guid.NewGuid())
            .WithVersion("1.0")
            .WithStorage(new StorageDescriptor
            {
                Protocol = "ipfs",
                IntegrityProof = "ni:///sha-256;abcdef",
                MediaType = "application/json",
                SizeBytes = 1234,
                Location = new StorageLocation
                {
                    Provider = "IPFS",
                    Region = "us-east",
                    Jurisdiction = "US"
                }
            })
            .WithIdentity(new IdentityDescriptor
            {
                Org = "did:example:org",
                Process = "unit-test",
                Artifact = artifact
            })
            .WithTimestamp(DateTimeOffset.UtcNow)
            .WithAnchor(new AnchorProof
            {
                Chain = "stellar:testnet",
                TransactionHash = "placeholder",
                HashAlgorithm = "sha-256",
                AnchoredAt = DateTimeOffset.UtcNow
            })
            .WithSignatures(Array.Empty<SignatureProof>())
            .Build();
    }
}
