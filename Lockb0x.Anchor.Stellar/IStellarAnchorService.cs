using System.Threading;
using System.Threading.Tasks;
using Lockb0x.Core.Models;

namespace Lockb0x.Anchor.Stellar;

/// <summary>
/// Provides anchoring and verification helpers for committing Codex Entries to the Stellar ledger.
/// </summary>
public interface IStellarAnchorService
{
    /// <summary>
    /// Anchors the supplied Codex Entry on the configured Stellar network and returns the resulting proof.
    /// </summary>
    Task<AnchorProof> AnchorAsync(
        CodexEntry entry,
        string network = "testnet",
        string? stellarPublicKey = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies that the supplied anchor proof exists on the configured Stellar network and matches the Codex Entry hash.
    /// </summary>
    Task<bool> VerifyAnchorAsync(
        AnchorProof anchor,
        CodexEntry entry,
        string network = "testnet",
        string? stellarPublicKey = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a block explorer URL for the supplied anchor proof.
    /// </summary>
    Task<string> GetTransactionUrlAsync(
        AnchorProof anchor,
        string network = "testnet",
        CancellationToken cancellationToken = default);
}
