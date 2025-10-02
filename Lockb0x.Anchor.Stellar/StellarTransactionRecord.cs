using System;
using System.Security.Cryptography;

namespace Lockb0x.Anchor.Stellar;

/// <summary>
/// Represents a Stellar transaction relevant for anchoring.
/// </summary>
public sealed class StellarTransactionRecord
{
    public StellarTransactionRecord(string hash, byte[] memoBytes, string memoType, DateTimeOffset? ledgerCloseTime, string? sourceAccount = null)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            throw new ArgumentException("Transaction hash must be provided.", nameof(hash));
        }

        Hash = hash;
        MemoBytes = memoBytes ?? Array.Empty<byte>();
        MemoType = memoType ?? "hash";
        LedgerCloseTime = ledgerCloseTime;
        SourceAccount = sourceAccount;
    }

    public string Hash { get; }

    public ReadOnlyMemory<byte> MemoBytes { get; }

    public string MemoType { get; }

    public DateTimeOffset? LedgerCloseTime { get; }

    public string? SourceAccount { get; }

    public static StellarTransactionRecord CreateSynthetic(StellarMemo memo, StellarNetworkOptions network, DateTimeOffset timestamp)
    {
        var hashBytes = SHA256.HashData(memo.Value.Span);
        var hash = Convert.ToHexString(hashBytes).ToLowerInvariant();
        return new StellarTransactionRecord(hash, memo.Value.ToArray(), memo.MemoType, timestamp, network.DefaultAccountPublicKey);
    }
}
