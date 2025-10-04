using System;

namespace Lockb0x.Anchor.Stellar;

/// <summary>
/// Represents configuration for a Stellar network/environment.
/// </summary>
public sealed class StellarNetworkOptions
{
    /// <summary>
    /// Human-readable network name (e.g., "testnet", "pubnet", "dry-run").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// CAIP-2 compliant chain identifier (e.g., "stellar:testnet").
    /// </summary>
    public required string ChainId { get; init; }

    /// <summary>
    /// Horizon endpoint base URI.
    /// </summary>
    public required Uri HorizonEndpoint { get; init; }

    /// <summary>
    /// Optional public key used when callers do not supply one explicitly.
    /// </summary>
    public string? DefaultAccountPublicKey { get; init; }

    /// <summary>
    /// Indicates whether this network is purely synthetic and should not attempt real Horizon submissions.
    /// </summary>
    public bool IsDryRun { get; init; }

    /// <summary>
    /// When <c>true</c> a signed transaction envelope is required before submission.
    /// </summary>
    public bool RequireSignedEnvelope { get; init; }

    /// <summary>
    /// Template for explorer URLs. Supports {network} and {hash} placeholders.
    /// </summary>
    public string ExplorerTransactionUrlTemplate { get; init; } = "https://stellar.expert/explorer/{network}/tx/{hash}";

    public string GetExplorerUrl(string transactionHash)
    {
        if (string.IsNullOrWhiteSpace(transactionHash))
        {
            throw new ArgumentException("Transaction hash must be provided.", nameof(transactionHash));
        }

        if (string.IsNullOrWhiteSpace(ExplorerTransactionUrlTemplate))
        {
            return new Uri(HorizonEndpoint, $"/transactions/{transactionHash}").ToString();
        }

        return ExplorerTransactionUrlTemplate
            .Replace("{network}", Name, StringComparison.OrdinalIgnoreCase)
            .Replace("{hash}", transactionHash, StringComparison.Ordinal);
    }

    public static StellarNetworkOptions CreateTestNet() => new()
    {
        Name = "testnet",
        ChainId = "stellar:testnet",
        HorizonEndpoint = new Uri("https://horizon-testnet.stellar.org"),
        RequireSignedEnvelope = false
    };

    public static StellarNetworkOptions CreatePubNet() => new()
    {
        Name = "pubnet",
        ChainId = "stellar:pubnet",
        HorizonEndpoint = new Uri("https://horizon.stellar.org"),
        RequireSignedEnvelope = true
    };

    public static StellarNetworkOptions CreateDryRun() => new()
    {
        Name = "dry-run",
        ChainId = "stellar:dry-run",
        HorizonEndpoint = new Uri("https://stellar.example/dry-run"),
        IsDryRun = true,
        RequireSignedEnvelope = false
    };
}
