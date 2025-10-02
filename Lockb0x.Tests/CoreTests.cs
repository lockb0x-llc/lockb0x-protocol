using System;
using System.Text.Json;
using System.Security.Cryptography;
using Xunit;
using Lockb0x.Core.Models;
using Lockb0x.Core.Canonicalization;
using Lockb0x.Core.Utilities;
using Lockb0x.Core.Validation;
using Lockb0x.Core.Revision;

namespace Lockb0x.Tests;

public class CoreTests
{
    // 1. Data Model

    [Fact]
    public void CodexEntry_Builder_Constructs_Valid_Entry()
    {
        var entry = new CodexEntryBuilder()
            .WithId(Guid.NewGuid())
            .WithVersion("1.0")
            .WithStorage(new StorageDescriptor
            {
                Protocol = "ipfs",
                IntegrityProof = NiUri.Create(new byte[] { 1, 2, 3 }),
                MediaType = "application/pdf",
                SizeBytes = 12345,
                Location = new StorageLocation
                {
                    Region = "us-central1",
                    Jurisdiction = "US",
                    Provider = "IPFS"
                }
            })
            .WithIdentity(new IdentityDescriptor
            {
                Org = "did:example:123",
                Artifact = "EmployeeHandbook-v1"
            })
            .WithTimestamp(DateTimeOffset.UtcNow)
            .WithAnchor(new AnchorProof
            {
                Chain = "stellar:pubnet",
                TransactionHash = "abcdef123456",
                HashAlgorithm = "sha-256"
            })
            .WithSignatures(new[] {
                new SignatureProof {
                    ProtectedHeader = new SignatureProtectedHeader {
                        Algorithm = "EdDSA"
                    },
                    Signature = "deadbeef"
                }
            })
            .Build();

        Assert.NotNull(entry);
        Assert.Equal("ipfs", entry.Storage.Protocol);
        Assert.Equal("did:example:123", entry.Identity.Org);
        Assert.Single(entry.Signatures);
    }


    [Fact]
    public void CodexEntry_Serialization_Deserialization_RoundTrip()
    {
        var entry = new CodexEntryBuilder()
            .WithId(Guid.NewGuid())
            .WithVersion("1.0")
            .WithStorage(new StorageDescriptor
            {
                Protocol = "ipfs",
                IntegrityProof = NiUri.Create(new byte[] { 1, 2, 3 }),
                MediaType = "application/pdf",
                SizeBytes = 12345,
                Location = new StorageLocation
                {
                    Region = "us-central1",
                    Jurisdiction = "US",
                    Provider = "IPFS"
                }
            })
            .WithIdentity(new IdentityDescriptor
            {
                Org = "did:example:123",
                Artifact = "EmployeeHandbook-v1"
            })
            .WithTimestamp(DateTimeOffset.UtcNow)
            .WithAnchor(new AnchorProof
            {
                Chain = "stellar:pubnet",
                TransactionHash = "abcdef123456",
                HashAlgorithm = "sha-256"
            })
            .WithSignatures(new[] {
                new SignatureProof {
                    ProtectedHeader = new SignatureProtectedHeader {
                        Algorithm = "EdDSA"
                    },
                    Signature = "deadbeef"
                }
            })
            .Build();

        var json = JsonSerializer.Serialize(entry);
        var deserialized = JsonSerializer.Deserialize<CodexEntry>(json);
        Assert.NotNull(deserialized);
        Assert.Equal(entry.Id, deserialized.Id);
        Assert.Equal(entry.Storage.Protocol, deserialized.Storage.Protocol);
    }

    // 2. Canonicalization

    [Fact]
    public void JcsCanonicalizer_Canonicalizes_Json_Correctly()
    {
        var obj = new { b = 2, a = 1 };
        var canonicalizer = new JcsCanonicalizer();
        var canonical = canonicalizer.Canonicalize(obj);
        Assert.Equal("{\"a\":1,\"b\":2}", canonical);
    }


    [Fact]
    public void JcsCanonicalizer_Hash_Matches_Known_Vector()
    {
        var obj = new { foo = "bar" };
        var canonicalizer = new JcsCanonicalizer();
        var hash = canonicalizer.Hash(obj, HashAlgorithmName.SHA256);
        var expected = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes("{\"foo\":\"bar\"}"));
        Assert.Equal(expected, hash);
    }

    // 3. ni-URI Helpers

    [Fact]
    public void NiUri_Create_And_Parse_Works_For_SHA256()
    {
        var data = new byte[] { 1, 2, 3 };
        var uri = NiUri.Create(data, HashAlgorithmName.SHA256);
        Assert.StartsWith("ni:///sha-256;", uri);
        Assert.True(NiUri.TryParse(uri, out var alg, out var digest));
        Assert.Equal("sha-256", alg);
        Assert.NotEmpty(digest);
    }


    [Fact]
    public void NiUri_Invalid_Input_Throws()
    {
        Assert.False(NiUri.TryParse("ni:///notarealalg;badbase64", out var alg, out var digest));
        Assert.Equal(string.Empty, alg);
        Assert.Empty(digest);
    }

    // 4. Validation

    [Fact]
    public void CodexEntryValidator_Validates_Required_Fields()
    {
        var entry = new CodexEntryBuilder()
            .WithId(Guid.NewGuid())
            .WithVersion("1.0")
            .WithStorage(new StorageDescriptor
            {
                Protocol = "ipfs",
                IntegrityProof = "ni:///sha-256;invalid",
                MediaType = "application/pdf",
                SizeBytes = 0,
                Location = new StorageLocation
                {
                    Region = "",
                    Jurisdiction = "",
                    Provider = ""
                }
            })
            .WithIdentity(new IdentityDescriptor
            {
                Org = "not-a-did-or-account",
                Artifact = ""
            })
            .WithTimestamp(DateTimeOffset.UtcNow)
            .WithAnchor(new AnchorProof
            {
                Chain = "invalidchain",
                TransactionHash = "nothex",
                HashAlgorithm = ""
            })
            .WithSignatures(new[] {
                new SignatureProof {
                    ProtectedHeader = new SignatureProtectedHeader {
                        Algorithm = ""
                    },
                    Signature = ""
                }
            })
            .Build();

        var validator = new CodexEntryValidator();
        var result = validator.Validate(entry);
        Assert.False(result.Success);
        Assert.NotEmpty(result.Errors);
    }


    [Fact]
    public void CodexEntryValidator_Validates_MultiSig_And_Anchor()
    {
        var entry = new CodexEntryBuilder()
            .WithId(Guid.NewGuid())
            .WithVersion("1.0")
            .WithStorage(new StorageDescriptor
            {
                Protocol = "ipfs",
                IntegrityProof = NiUri.Create(new byte[] { 1, 2, 3 }),
                MediaType = "application/pdf",
                SizeBytes = 12345,
                Location = new StorageLocation
                {
                    Region = "us-central1",
                    Jurisdiction = "US",
                    Provider = "IPFS"
                }
            })
            .WithIdentity(new IdentityDescriptor
            {
                Org = "did:example:123",
                Artifact = "EmployeeHandbook-v1"
            })
            .WithTimestamp(DateTimeOffset.UtcNow)
            .WithAnchor(new AnchorProof
            {
                Chain = "stellar:pubnet",
                TransactionHash = "abcdef123456",
                HashAlgorithm = "sha-256"
            })
            .WithSignatures(new[] {
                new SignatureProof {
                    ProtectedHeader = new SignatureProtectedHeader {
                        Algorithm = "EdDSA"
                    },
                    Signature = "deadbeef"
                },
                new SignatureProof {
                    ProtectedHeader = new SignatureProtectedHeader {
                        Algorithm = "EdDSA"
                    },
                    Signature = "cafebabe"
                }
            })
            .Build();

        var validator = new CodexEntryValidator();
        var result = validator.Validate(entry);
        Assert.True(result.Success);
    }

    // 5. Revision & Provenance

    [Fact]
    public void RevisionGraph_Traverses_Chain_And_Detects_Cycles()
    {
        var entry1 = new CodexEntryBuilder()
            .WithId("11111111-1111-4111-8111-111111111111")
            .WithVersion("1.0")
            .WithStorage(new StorageDescriptor
            {
                Protocol = "ipfs",
                IntegrityProof = NiUri.Create(new byte[] { 1 }),
                MediaType = "application/pdf",
                SizeBytes = 1,
                Location = new StorageLocation
                {
                    Region = "us-central1",
                    Jurisdiction = "US",
                    Provider = "IPFS"
                }
            })
            .WithIdentity(new IdentityDescriptor
            {
                Org = "did:example:123",
                Artifact = "A"
            })
            .WithTimestamp(DateTimeOffset.UtcNow)
            .WithAnchor(new AnchorProof
            {
                Chain = "stellar:pubnet",
                TransactionHash = "a",
                HashAlgorithm = "sha-256"
            })
            .WithSignatures(new[] {
                new SignatureProof {
                    ProtectedHeader = new SignatureProtectedHeader {
                        Algorithm = "EdDSA"
                    },
                    Signature = "sig1"
                }
            })
            .Build();

        var entry2 = new CodexEntryBuilder()
            .WithId("22222222-2222-4222-8222-222222222222")
            .WithPreviousId(entry1.Id)
            .WithVersion("1.0")
            .WithStorage(entry1.Storage)
            .WithIdentity(entry1.Identity)
            .WithTimestamp(DateTimeOffset.UtcNow)
            .WithAnchor(entry1.Anchor)
            .WithSignatures(entry1.Signatures)
            .Build();

        var entry3 = new CodexEntryBuilder()
            .WithId("33333333-3333-4333-8333-333333333333")
            .WithPreviousId(entry2.Id)
            .WithVersion("1.0")
            .WithStorage(entry1.Storage)
            .WithIdentity(entry1.Identity)
            .WithTimestamp(DateTimeOffset.UtcNow)
            .WithAnchor(entry1.Anchor)
            .WithSignatures(entry1.Signatures)
            .Build();

        // Introduce a cycle
        var entry1Cycle = new CodexEntryBuilder()
            .WithId(entry1.Id)
            .WithPreviousId(entry3.Id)
            .WithVersion("1.0")
            .WithStorage(entry1.Storage)
            .WithIdentity(entry1.Identity)
            .WithTimestamp(DateTimeOffset.UtcNow)
            .WithAnchor(entry1.Anchor)
            .WithSignatures(entry1.Signatures)
            .Build();

        var entries = new[] { entry1, entry2, entry3, entry1Cycle };
        CodexEntry Resolver(string id) => Array.Find(entries, e => e.Id == id);
        var graph = new RevisionGraph();
        var result = graph.Traverse(entry3, Resolver);
        Assert.Equal(3, result.Chain.Count);
        Assert.True(result.Issues.Count == 0);

        // Test cycle detection
        var resultCycle = graph.Traverse(entry1Cycle, Resolver);
        Assert.Contains(resultCycle.Issues, i => i.Code == "core.revision.cycle_detected");
    }

    // 6. Extensibility

    [Fact]
    public void CodexEntry_Extensions_Supports_Custom_Metadata()
    {
        var extensions = JsonDocument.Parse("{\"custom\":123}");
        var entry = new CodexEntryBuilder()
            .WithId(Guid.NewGuid())
            .WithVersion("1.0")
            .WithStorage(new StorageDescriptor
            {
                Protocol = "ipfs",
                IntegrityProof = NiUri.Create(new byte[] { 1 }),
                MediaType = "application/pdf",
                SizeBytes = 1,
                Location = new StorageLocation
                {
                    Region = "us-central1",
                    Jurisdiction = "US",
                    Provider = "IPFS"
                }
            })
            .WithIdentity(new IdentityDescriptor
            {
                Org = "did:example:123",
                Artifact = "A"
            })
            .WithTimestamp(DateTimeOffset.UtcNow)
            .WithAnchor(new AnchorProof
            {
                Chain = "stellar:pubnet",
                TransactionHash = "a",
                HashAlgorithm = "sha-256"
            })
            .WithSignatures(new[] {
                new SignatureProof {
                    ProtectedHeader = new SignatureProtectedHeader {
                        Algorithm = "EdDSA"
                    },
                    Signature = "sig1"
                }
            })
            .WithExtensions(extensions)
            .Build();

        Assert.NotNull(entry.Extensions);
        Assert.Equal(123, entry.Extensions.RootElement.GetProperty("custom").GetInt32());
    }

    // 7. Deterministic Vectors

    [Fact]
    public void CodexEntry_Matches_AppendixA_Flows()
    {
        // Example: IPFS
        var entry = new CodexEntryBuilder()
            .WithId(Guid.NewGuid())
            .WithVersion("1.0")
            .WithStorage(new StorageDescriptor
            {
                Protocol = "ipfs",
                IntegrityProof = "ni:///sha-256;b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9",
                MediaType = "application/pdf",
                SizeBytes = 12345,
                Location = new StorageLocation
                {
                    Region = "us-east-1",
                    Jurisdiction = "US",
                    Provider = "IPFS"
                }
            })
            .WithIdentity(new IdentityDescriptor
            {
                Org = "Org123",
                Artifact = "EmployeeHandbook-v1"
            })
            .WithTimestamp(DateTimeOffset.UtcNow)
            .WithAnchor(new AnchorProof
            {
                Chain = "stellar:pubnet",
                TransactionHash = "abcdef123456",
                HashAlgorithm = "sha-256"
            })
            .WithSignatures(new[] {
                new SignatureProof {
                    ProtectedHeader = new SignatureProtectedHeader {
                        Algorithm = "EdDSA"
                    },
                    Signature = "deadbeef"
                }
            })
            .Build();

        var validator = new CodexEntryValidator();
        var result = validator.Validate(entry);
        Assert.True(result.Success);
    }
}
