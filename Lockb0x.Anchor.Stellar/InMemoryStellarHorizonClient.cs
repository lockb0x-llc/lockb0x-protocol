using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Lockb0x.Anchor.Stellar;

/// <summary>
/// Simple in-memory Horizon client used for tests and dry-run environments.
/// </summary>
public sealed class InMemoryStellarHorizonClient : IStellarHorizonClient
{
    private readonly ConcurrentDictionary<string, StellarTransactionRecord> _transactions = new();
    private readonly ConcurrentDictionary<string, string> _memoIndex = new();

    public Task<StellarTransactionRecord?> FindTransactionByMemoAsync(StellarMemo memo, StellarNetworkOptions network, CancellationToken cancellationToken)
    {
        var key = BuildMemoKey(network.Name, memo.Fingerprint);
        if (_memoIndex.TryGetValue(key, out var hash))
        {
            return GetTransactionAsync(hash, network, cancellationToken);
        }

        return Task.FromResult<StellarTransactionRecord?>(null);
    }

    public Task<StellarTransactionRecord?> GetTransactionAsync(string transactionHash, StellarNetworkOptions network, CancellationToken cancellationToken)
    {
        var key = BuildTransactionKey(network.Name, transactionHash);
        if (_transactions.TryGetValue(key, out var record))
        {
            return Task.FromResult<StellarTransactionRecord?>(record);
        }

        return Task.FromResult<StellarTransactionRecord?>(null);
    }

    public Task<StellarTransactionRecord> SubmitTransactionAsync(StellarSubmitTransactionRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var memo = request.Memo ?? throw new ArgumentException("Memo must be provided for Stellar submission.", nameof(request));
        var network = request.Network ?? throw new ArgumentException("Network configuration is required.", nameof(request));

        var hashBytes = SHA256.HashData(memo.Value.Span);
        var hash = Convert.ToHexString(hashBytes).ToLowerInvariant();
        var timestamp = request.RequestedAt ?? DateTimeOffset.UtcNow;
        var record = new StellarTransactionRecord(hash, memo.Value.ToArray(), memo.MemoType, timestamp, request.PublicKey);

        _transactions[BuildTransactionKey(network.Name, hash)] = record;
        _memoIndex[BuildMemoKey(network.Name, memo.Fingerprint)] = hash;

        return Task.FromResult(record);
    }

    private static string BuildMemoKey(string network, string fingerprint) => $"{Normalize(network)}::{fingerprint}";

    private static string BuildTransactionKey(string network, string hash) => $"{Normalize(network)}::{hash}";

    private static string Normalize(string value) => value?.ToLowerInvariant() ?? string.Empty;
}
