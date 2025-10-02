using System;

namespace Lockb0x.Anchor.Stellar;

/// <summary>
/// Provides the data required to construct a Stellar transaction envelope.
/// </summary>
public sealed record AnchorTransactionContext(
    StellarNetworkOptions Network,
    StellarMemo Memo,
    ReadOnlyMemory<byte> EntryHash,
    string PublicKey);
