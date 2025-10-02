using Lockb0x.Core.Canonicalization;
using Lockb0x.Core.Models;

namespace Lockb0x.Anchor.Stellar;

/// <summary>
/// Produces Stellar memo payloads for Codex Entry anchors.
/// </summary>
public interface IMemoStrategy
{
    /// <summary>
    /// Creates a deterministic memo payload for the supplied Codex Entry and Stellar account.
    /// </summary>
    StellarMemo CreateMemo(CodexEntry entry, string stellarPublicKey, IJsonCanonicalizer canonicalizer);
}
