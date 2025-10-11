using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Lockb0x.Core.Canonicalization;
using Lockb0x.Core.Models;

namespace Lockb0x.Anchor.Stellar;

public sealed class StellarAnchorService : IStellarAnchorService
{
    private readonly IJsonCanonicalizer _canonicalizer;
    private readonly IStellarHorizonClient _horizonClient;
    private readonly IMemoStrategy _memoStrategy;
    private readonly StellarAnchorServiceOptions _options;
    private readonly IClock _clock;

    public StellarAnchorService(
        IJsonCanonicalizer canonicalizer,
        IStellarHorizonClient horizonClient,
        IMemoStrategy? memoStrategy = null,
        StellarAnchorServiceOptions? options = null,
        IClock? clock = null)
    {
        _canonicalizer = canonicalizer ?? throw new ArgumentNullException(nameof(canonicalizer));
        _horizonClient = horizonClient ?? throw new ArgumentNullException(nameof(horizonClient));
        _memoStrategy = memoStrategy ?? new Md5AccountMemoStrategy();
        _options = options ?? StellarAnchorServiceOptions.CreateDefault();
        _clock = clock ?? SystemClock.Instance;
    }

    public async Task<AnchorProof> AnchorAsync(
        CodexEntry entry,
        string network = "testnet",
        string? stellarPublicKey = null,
        CancellationToken cancellationToken = default)
    {
        if (entry is null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        var networkOptions = ResolveNetwork(network);
        var account = ResolvePublicKey(stellarPublicKey, networkOptions);
        var entryHash = ComputeEntryHash(entry);
        var memo = _memoStrategy.CreateMemo(entry, account, _canonicalizer);

        var existing = await _horizonClient.FindTransactionByMemoAsync(memo, networkOptions, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            return BuildAnchorProof(existing, networkOptions);
        }

        string? envelope = null;
        if (_options.TransactionEnvelopeFactory is not null)
        {
            var context = new AnchorTransactionContext(networkOptions, memo, entryHash, account);
            envelope = await _options.TransactionEnvelopeFactory(context, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(envelope))
            {
                envelope = null;
            }
        }
        else if (networkOptions.RequireSignedEnvelope && !networkOptions.IsDryRun)
        {
            throw new StellarAnchorException(
                "anchor.envelope_required",
                $"Network '{networkOptions.Name}' requires a signed transaction envelope.");
        }

        var request = new StellarSubmitTransactionRequest
        {
            Network = networkOptions,
            Memo = memo,
            EntryHash = entryHash,
            PublicKey = account,
            EnvelopeXdr = envelope,
            RequestedAt = _clock.UtcNow
        };

        var record = await _horizonClient.SubmitTransactionAsync(request, cancellationToken).ConfigureAwait(false);
        return BuildAnchorProof(record, networkOptions);
    }

    public async Task<bool> VerifyAnchorAsync(
        AnchorProof anchor,
        CodexEntry entry,
        string network = "testnet",
        string? stellarPublicKey = null,
        CancellationToken cancellationToken = default)
    {
        if (anchor is null)
        {
            throw new ArgumentNullException(nameof(anchor));
        }

        if (entry is null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        var networkOptions = ResolveNetwork(network);
        if (!string.Equals(anchor.Chain, networkOptions.ChainId, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.Equals(anchor.HashAlgorithm, "SHA256", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var account = ResolvePublicKey(stellarPublicKey, networkOptions);
        var memo = _memoStrategy.CreateMemo(entry, account, _canonicalizer);

        var record = await _horizonClient.GetTransactionAsync(anchor.Reference, networkOptions, cancellationToken).ConfigureAwait(false);
        if (record is null)
        {
            return false;
        }

        if (!memo.Matches(record.MemoBytes.Span))
        {
            return false;
        }

        if (anchor.AnchoredAt.HasValue && record.LedgerCloseTime.HasValue)
        {
            // Allow for minor clock drift (±5 minutes).
            var difference = (anchor.AnchoredAt.Value - record.LedgerCloseTime.Value).Duration();
            if (difference > TimeSpan.FromMinutes(5))
            {
                return false;
            }
        }

        return true;
    }

    public Task<string> GetTransactionUrlAsync(
        AnchorProof anchor,
        string network = "testnet",
        CancellationToken cancellationToken = default)
    {
        if (anchor is null)
        {
            throw new ArgumentNullException(nameof(anchor));
        }

        var networkOptions = ResolveNetwork(network);
        var url = networkOptions.GetExplorerUrl(anchor.Reference);
        return Task.FromResult(url);
    }

    private StellarNetworkOptions ResolveNetwork(string? network)
    {
        var key = string.IsNullOrWhiteSpace(network) ? _options.DefaultNetwork : network;
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new StellarAnchorException("anchor.network_missing", "No Stellar network was specified and no default is configured.");
        }

        if (_options.Networks.TryGetValue(key, out var configured))
        {
            return configured;
        }

        if (_options.AllowUnknownNetworks)
        {
            var synthetic = new StellarNetworkOptions
            {
                Name = key,
                ChainId = $"stellar:{key.ToLowerInvariant()}",
                HorizonEndpoint = new Uri($"https://{key}.stellar.invalid"),
                IsDryRun = true,
                RequireSignedEnvelope = false
            };
            _options.Networks[key] = synthetic;
            return synthetic;
        }

        throw new StellarAnchorException("anchor.network_unknown", $"No Stellar network configuration found for '{key}'.");
    }

    private static string ResolvePublicKey(string? candidate, StellarNetworkOptions network)
    {
        var value = string.IsNullOrWhiteSpace(candidate) ? network.DefaultAccountPublicKey : candidate;
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new StellarAnchorException("anchor.public_key_missing", "A Stellar public key must be provided either explicitly or via network configuration.");
        }

        return value.Trim();
    }

    private byte[] ComputeEntryHash(CodexEntry entry)
    {
        try
        {
            return _canonicalizer.Hash(entry, HashAlgorithmName.SHA256);
        }
        catch (Exception ex)
        {
            throw new StellarAnchorException("anchor.hash_failed", "Failed to compute Codex Entry hash for anchoring.", ex);
        }
    }

    private AnchorProof BuildAnchorProof(StellarTransactionRecord record, StellarNetworkOptions network)
    {
        return new AnchorProof
        {
            Chain = network.ChainId,
            Reference = record.Hash,
            HashAlgorithm = "SHA256",
            AnchoredAt = record.LedgerCloseTime ?? _clock.UtcNow
        };
    }
}
