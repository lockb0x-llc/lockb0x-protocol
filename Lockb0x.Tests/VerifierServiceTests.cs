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
        var entry = new CodexEntry();
        var result = await service.VerifyAsync(entry);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Validation not implemented."));
        Assert.Contains(result.Errors, e => e.Contains("Integrity proof validation failed"));
        Assert.Contains(result.Errors, e => e.Contains("Signature validation failed"));
        Assert.Contains(result.Warnings, w => w.Contains("Storage proof validation not implemented."));
        Assert.Contains(result.Warnings, w => w.Contains("Anchor proof validation not implemented."));
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
        var entry = new CodexEntry
        {
            Integrity = { new IntegrityProof { Algorithm = "sha-256", Hash = "abc" } },
            Signatures = { new SignatureProof { Algorithm = "ed25519", Value = "sig", Signer = "did:example:123" } }
        };
        var result = await service.VerifyAsync(entry);
        // Schema validation will still fail due to not implemented, but integrity/signature stubs will pass
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Validation not implemented."));
        Assert.DoesNotContain(result.Errors, e => e.Contains("Integrity proof validation failed"));
        Assert.DoesNotContain(result.Errors, e => e.Contains("Signature validation failed"));
    }
}
