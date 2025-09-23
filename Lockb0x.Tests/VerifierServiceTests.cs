using Lockb0x.Verifier;
using Lockb0x.Core;
using Xunit;
using System;
using System.Threading.Tasks;

namespace Lockb0x.Tests;

public class VerifierServiceTests
{

    [Fact]
    public async Task VerifierService_Returns_Invalid_For_Empty_Entry()
    {
        var service = new VerifierService();
        var entry = new Lockb0x.Core.CodexEntry();
        var result = await service.VerifyAsync(entry);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Validation not implemented."));
        Assert.Contains(result.Errors, e => e.Contains("Integrity proof validation failed"));
        Assert.Contains(result.Errors, e => e.Contains("Signature validation failed"));
        // Warnings should be present but may be empty - let's check if steps are recorded
        Assert.Contains(result.Steps, s => s == "Schema validation");
        Assert.Contains(result.Steps, s => s == "Integrity proof validation");
        Assert.Contains(result.Steps, s => s == "Signature validation");
        Assert.Contains(result.Steps, s => s == "Storage proof validation");
        Assert.Contains(result.Steps, s => s == "Anchor proof validation");
    }

    [Fact]
    public async Task VerifierService_Returns_Valid_For_Minimal_Valid_Entry()
    {
        var service = new VerifierService();
        var entry = new Lockb0x.Core.CodexEntry
        {
            Integrity = { new Lockb0x.Core.IntegrityProof { Algorithm = "sha-256", Hash = "abc" } },
            Signatures = { new Lockb0x.Core.SignatureProof { Algorithm = "ed25519", Value = "sig", Signer = "did:example:123" } }
        };
        var result = await service.VerifyAsync(entry);
        // Schema validation will still fail due to not implemented, but integrity/signature stubs will pass
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Validation not implemented."));
        Assert.DoesNotContain(result.Errors, e => e.Contains("Integrity proof validation failed"));
        Assert.DoesNotContain(result.Errors, e => e.Contains("Signature validation failed"));
    }

    [Fact]
    public void CodexEntryValidator_Should_Reject_Unknown_Fields()
    {
        // This test validates that schema validation rejects entries with unknown fields
        var jsonWithUnknownField = """
        {
            "id": "550e8400-e29b-41d4-a716-446655440000",
            "version": "1.0.0",
            "storage": {
                "protocol": "ipfs",
                "integrity_proof": "ni:///sha-256;abc123",
                "media_type": "application/pdf",
                "size_bytes": 1024,
                "location": {
                    "region": "us-west-1",
                    "jurisdiction": "US/CA",
                    "provider": "IPFS"
                }
            },
            "identity": {
                "org": "did:example:123",
                "process": "did:example:456", 
                "artifact": "workorder-789"
            },
            "timestamp": "2025-01-01T00:00:00Z",
            "anchor": {
                "chain": "stellar:pubnet",
                "tx_hash": "abc123",
                "hash_alg": "SHA256"
            },
            "signatures": [{
                "protected": {"alg": "EdDSA", "kid": "stellar:GA123"},
                "signature": "base64sig"
            }],
            "unknown_field": "this should be rejected"
        }
        """;
        
        var isValid = CodexEntryValidator.ValidateJson(jsonWithUnknownField, out var errors);
        Assert.False(isValid);
        Assert.Contains(errors, e => e.Contains("unknown_field") || e.Contains("additional") || e.Contains("unexpected"));
    }

    [Fact]
    public void CodexEntryValidator_Should_Accept_Valid_Entry_Without_Unknown_Fields()
    {
        // This test validates that schema validation accepts valid entries
        var validJson = """
        {
            "id": "550e8400-e29b-41d4-a716-446655440000",
            "version": "1.0.0",
            "storage": {
                "protocol": "ipfs",
                "integrity_proof": "ni:///sha-256;abc123",
                "media_type": "application/pdf",
                "size_bytes": 1024,
                "location": {
                    "region": "us-west-1",
                    "jurisdiction": "US/CA",
                    "provider": "IPFS"
                }
            },
            "identity": {
                "org": "did:example:123",
                "process": "did:example:456", 
                "artifact": "workorder-789"
            },
            "timestamp": "2025-01-01T00:00:00Z",
            "anchor": {
                "chain": "stellar:pubnet",
                "tx_hash": "abc123",
                "hash_alg": "SHA256"
            },
            "signatures": [{
                "protected": {"alg": "EdDSA", "kid": "stellar:GA123"},
                "signature": "base64sig"
            }]
        }
        """;
        
        var isValid = CodexEntryValidator.ValidateJson(validJson, out var errors);
        Assert.True(isValid);
        Assert.Empty(errors);
    }
}
