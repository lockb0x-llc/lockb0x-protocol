using System.Threading.Tasks;
using Lockb0x.Core;

namespace Lockb0x.Anchor.Stellar;

public interface IStellarAnchorService
{
    Task<AnchorProof> AnchorAsync(CodexEntry entry, string network = "testnet", string? stellarPublicKey = null);
    Task<bool> VerifyAnchorAsync(AnchorProof anchor, CodexEntry entry, string network = "testnet", string? stellarPublicKey = null);
    Task<string> GetTransactionUrlAsync(AnchorProof anchor, string network = "testnet");
}


public class StellarAnchorService : IStellarAnchorService
{

    public async Task<AnchorProof> AnchorAsync(CodexEntry entry, string network = "testnet", string? stellarPublicKey = null)
    {
        // 1. Hash the CodexEntry (canonicalized JSON, SHA-256)
        byte[] hash = ComputeEntryHash(entry);
        string hashHex = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();

        // 2. Generate Stellar memo identifier (MD5 + public key truncated to 28 bytes)
        string memoHex = GenerateStellarMemo(entry, stellarPublicKey ?? "GABC123DEFAULTKEY");

        // 3. Build a Stellar transaction with the memo (stubbed for dry-run/testnet)
        string txHash = IsDryRun(network)
            ? $"dryrun-{Guid.NewGuid()}"
            : "TODO: Implement real Stellar transaction submission";

        // 4. Populate AnchorProof
        var proof = new AnchorProof
        {
            ChainId = GetCaip2ChainId(network),
            TxHash = txHash,
            HashAlgorithm = "sha-256",
            AnchoredAt = DateTimeOffset.UtcNow
        };

        // 5. TODO: Integrate with Stellar SDK for real transaction submission with memoHex

        return await Task.FromResult(proof);
    }


    public async Task<bool> VerifyAnchorAsync(AnchorProof anchor, CodexEntry entry, string network = "testnet", string? stellarPublicKey = null)
    {
        // 1. Hash the CodexEntry
        byte[] hash = ComputeEntryHash(entry);
        string hashHex = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();

        // 2. For dry-run/testnet, just check txHash format
        if (IsDryRun(network))
        {
            return await Task.FromResult(anchor.TxHash.StartsWith("dryrun-"));
        }

        // 3. TODO: Query Stellar Horizon for transaction, check memo hash matches computed memo
        // The memo should match the result of GenerateStellarMemo(entry, stellarPublicKey)
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

    // Utility: Generate Stellar memo identifier (MD5 hash + public key, max 28 bytes)
    public static string GenerateStellarMemo(CodexEntry entry, string stellarPublicKey)
    {
        // 1. Compute MD5 hash of the canonicalized CodexEntry (16 bytes)
        byte[] md5Hash = ComputeEntryMd5Hash(entry);
        
        // 2. Take first 12 bytes of public key to fit within 28 byte constraint (16 + 12 = 28)
        byte[] publicKeyBytes = System.Text.Encoding.UTF8.GetBytes(stellarPublicKey);
        byte[] truncatedPublicKey = new byte[Math.Min(12, publicKeyBytes.Length)];
        Array.Copy(publicKeyBytes, truncatedPublicKey, truncatedPublicKey.Length);
        
        // 3. Combine MD5 hash and truncated public key
        byte[] combined = new byte[md5Hash.Length + truncatedPublicKey.Length];
        Array.Copy(md5Hash, 0, combined, 0, md5Hash.Length);
        Array.Copy(truncatedPublicKey, 0, combined, md5Hash.Length, truncatedPublicKey.Length);
        
        // 4. Convert to hex string (ensuring it fits within 28 bytes)
        string memoHex = BitConverter.ToString(combined).Replace("-", string.Empty).ToLowerInvariant();
        
        // Ensure we don't exceed 28 bytes (56 hex characters)
        if (memoHex.Length > 56)
        {
            memoHex = memoHex.Substring(0, 56);
        }
        
        return memoHex;
    }

    // Utility: Compute MD5 hash of CodexEntry for Stellar memo
    public static byte[] ComputeEntryMd5Hash(CodexEntry entry)
    {
        // Canonicalize CodexEntry to JSON (RFC 8785 JCS), then hash with MD5
        string canonicalJson;
        try
        {
            canonicalJson = Lockb0x.Core.JsonCanonicalizer.Canonicalize(entry);
        }
        catch (NotImplementedException)
        {
            throw new NotImplementedException("JsonCanonicalizer is not implemented.");
        }
        using var md5 = System.Security.Cryptography.MD5.Create();
        return md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(canonicalJson));
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
