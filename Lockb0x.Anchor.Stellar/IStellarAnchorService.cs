using System.Threading.Tasks;
using Lockb0x.Core;

namespace Lockb0x.Anchor.Stellar;

public interface IStellarAnchorService
{
    Task<AnchorProof> AnchorAsync(CodexEntry entry, string network = "testnet");
    Task<bool> VerifyAnchorAsync(AnchorProof anchor, CodexEntry entry, string network = "testnet");
    Task<string> GetTransactionUrlAsync(AnchorProof anchor, string network = "testnet");
}


public class StellarAnchorService : IStellarAnchorService
{

    public async Task<AnchorProof> AnchorAsync(CodexEntry entry, string network = "testnet")
    {
        // 1. Hash the CodexEntry (canonicalized JSON, SHA-256)
        byte[] hash = ComputeEntryHash(entry);
        string hashHex = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();

        // 2. Build a Stellar transaction with the hash as memo (stubbed for dry-run/testnet)
        string txHash = IsDryRun(network)
            ? $"dryrun-{Guid.NewGuid()}"
            : "TODO: Implement real Stellar transaction submission";

        // 3. Populate AnchorProof
        var proof = new AnchorProof
        {
            ChainId = GetCaip2ChainId(network),
            TxHash = txHash,
            HashAlgorithm = "sha-256",
            AnchoredAt = DateTimeOffset.UtcNow
        };

        // 4. TODO: Integrate with Stellar SDK for real transaction submission

        return await Task.FromResult(proof);
    }


    public async Task<bool> VerifyAnchorAsync(AnchorProof anchor, CodexEntry entry, string network = "testnet")
    {
        // 1. Hash the CodexEntry
        byte[] hash = ComputeEntryHash(entry);
        string hashHex = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();

        // 2. For dry-run/testnet, just check txHash format
        if (IsDryRun(network))
        {
            return await Task.FromResult(anchor.TxHash.StartsWith("dryrun-"));
        }

        // 3. TODO: Query Stellar Horizon for transaction, check memo hash matches entry hash
        // 4. TODO: Validate timestamp, chainId, and hash algorithm
        throw new NotImplementedException("Stellar Horizon integration not implemented.");
    }


    public Task<string> GetTransactionUrlAsync(AnchorProof anchor, string network = "testnet")
    {
        // For dry-run/testnet, return a stubbed URL
        if (IsDryRun(network))
        {
            return Task.FromResult($"https://stellar.explorer/{GetCaip2ChainId(network)}/tx/{anchor.TxHash}");
        }
        // TODO: Return real Stellar explorer URL for the transaction
        throw new NotImplementedException("Stellar explorer integration not implemented.");
    }

    // Utility: Hash a CodexEntry for anchoring (e.g., SHA-256 over canonicalized JSON)
    public static byte[] ComputeEntryHash(CodexEntry entry)
    {
        // Canonicalize CodexEntry to JSON (RFC 8785 JCS), then hash with SHA-256
        string canonicalJson;
        try
        {
            canonicalJson = Lockb0x.Core.JsonCanonicalizer.Canonicalize(entry);
        }
        catch (NotImplementedException)
        {
            throw new NotImplementedException("JsonCanonicalizer is not implemented.");
        }
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        return sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(canonicalJson));
    }

    // Utility: Map network to CAIP-2 chain ID (e.g., "stellar:testnet" or "stellar:pubnet")
    public static string GetCaip2ChainId(string network)
    {
        return network.ToLowerInvariant() switch
        {
            "testnet" => "stellar:testnet",
            "pubnet" => "stellar:pubnet",
            _ => $"stellar:{network.ToLowerInvariant()}"
        };
    }

    // Utility: Dry-run/testnet support (no real transaction submission)
    public static bool IsDryRun(string network) => network.ToLowerInvariant() == "dry-run" || network.ToLowerInvariant() == "testnet";
}
