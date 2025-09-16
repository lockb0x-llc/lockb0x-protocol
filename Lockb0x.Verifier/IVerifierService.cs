using System.Threading.Tasks;
using Lockb0x.Core;

namespace Lockb0x.Verifier;

public interface IVerifierService
{
    Task<VerificationResult> VerifyAsync(CodexEntry entry);
}

public class VerificationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Steps { get; set; } = new();
}

// Stub implementation
public class VerifierService : IVerifierService
{
    public Task<VerificationResult> VerifyAsync(CodexEntry entry) => throw new System.NotImplementedException();
}
