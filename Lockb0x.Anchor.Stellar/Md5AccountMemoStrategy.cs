using System;
using System.Security.Cryptography;
using System.Text;
using Lockb0x.Core.Canonicalization;
using Lockb0x.Core.Models;

namespace Lockb0x.Anchor.Stellar;

/// <summary>
/// Implements the default memo derivation strategy recommended by the Lockb0x specification:
/// MD5 of the canonical Codex Entry concatenated with the anchoring account identifier, truncated to 28 bytes.
/// </summary>
public sealed class Md5AccountMemoStrategy : IMemoStrategy
{
    public const int MemoSizeBytes = 28;

    public StellarMemo CreateMemo(CodexEntry entry, string stellarPublicKey, IJsonCanonicalizer canonicalizer)
    {
        if (entry is null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        if (canonicalizer is null)
        {
            throw new ArgumentNullException(nameof(canonicalizer));
        }

        if (string.IsNullOrWhiteSpace(stellarPublicKey))
        {
            throw new ArgumentException("Stellar public key must be provided for memo derivation.", nameof(stellarPublicKey));
        }

        byte[] md5Digest;
        try
        {
            md5Digest = canonicalizer.Hash(entry, HashAlgorithmName.MD5);
        }
        catch (Exception ex)
        {
            throw new StellarAnchorException("anchor.memo_hash_failed", "Failed to compute MD5 digest for memo derivation.", ex);
        }

        var publicKeyBytes = Encoding.UTF8.GetBytes(stellarPublicKey.Trim());
        var memoBytes = new byte[MemoSizeBytes];
        var copyLength = Math.Min(md5Digest.Length, MemoSizeBytes);
        Array.Copy(md5Digest, 0, memoBytes, 0, copyLength);

        if (copyLength < MemoSizeBytes)
        {
            var remaining = MemoSizeBytes - copyLength;
            if (publicKeyBytes.Length >= remaining)
            {
                Array.Copy(publicKeyBytes, 0, memoBytes, copyLength, remaining);
            }
            else
            {
                Array.Copy(publicKeyBytes, 0, memoBytes, copyLength, publicKeyBytes.Length);
                // Pad remaining bytes deterministically using zeros.
                for (var i = copyLength + publicKeyBytes.Length; i < MemoSizeBytes; i++)
                {
                    memoBytes[i] = 0x00;
                }
            }
        }

        var fingerprint = Convert.ToHexString(memoBytes).ToLowerInvariant();
        var text = Encoding.ASCII.GetString(memoBytes);
        return new StellarMemo("hash", memoBytes, fingerprint, text);
    }
}
