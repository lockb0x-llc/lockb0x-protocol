using System;

namespace Lockb0x.Anchor.Stellar;

/// <summary>
/// Represents a Stellar transaction submission request.
/// </summary>
public sealed class StellarSubmitTransactionRequest
{
    public required StellarNetworkOptions Network { get; init; }

    public required StellarMemo Memo { get; init; }

    public required ReadOnlyMemory<byte> EntryHash { get; init; }

    public required string PublicKey { get; init; }

    /// <summary>
    /// Optional signed transaction envelope (base64 XDR) ready for Horizon submission.
    /// </summary>
    public string? EnvelopeXdr { get; init; }

    /// <summary>
    /// Optional timestamp when the submission was requested.
    /// </summary>
    public DateTimeOffset? RequestedAt { get; init; }
}
