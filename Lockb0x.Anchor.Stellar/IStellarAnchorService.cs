using System.Threading.Tasks;
using Lockb0x.Core;

namespace Lockb0x.Anchor.Stellar;

public interface IStellarAnchorService
{
    Task<AnchorProof> AnchorAsync(CodexEntry entry, string network = "testnet");
    Task<bool> VerifyAnchorAsync(AnchorProof anchor, CodexEntry entry, string network = "testnet");
    Task<string> GetTransactionUrlAsync(AnchorProof anchor, string network = "testnet");
}

// Stub implementation
public class StellarAnchorService : IStellarAnchorService
{
    public Task<AnchorProof> AnchorAsync(CodexEntry entry, string network = "testnet") => throw new System.NotImplementedException();
    public Task<bool> VerifyAnchorAsync(AnchorProof anchor, CodexEntry entry, string network = "testnet") => throw new System.NotImplementedException();
    public Task<string> GetTransactionUrlAsync(AnchorProof anchor, string network = "testnet") => throw new System.NotImplementedException();
}
