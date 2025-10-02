using System;
using System.Linq;

namespace Lockb0x.Anchor.Stellar;

/// <summary>
/// Represents the memo payload used for Stellar anchoring.
/// </summary>
public sealed class StellarMemo
{
    public StellarMemo(string memoType, byte[] value, string fingerprint, string? textRepresentation = null)
    {
        if (string.IsNullOrWhiteSpace(memoType))
        {
            throw new ArgumentException("Memo type must be provided.", nameof(memoType));
        }

        MemoType = memoType;
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Fingerprint = fingerprint ?? throw new ArgumentNullException(nameof(fingerprint));
        TextRepresentation = textRepresentation;
    }

    /// <summary>
    /// Horizon memo type (text, hash, id, return).
    /// </summary>
    public string MemoType { get; }

    /// <summary>
    /// Raw memo bytes (at most 28 bytes for text payloads).
    /// </summary>
    public ReadOnlyMemory<byte> Value { get; }

    /// <summary>
    /// Stable memo fingerprint used for idempotency lookups.
    /// </summary>
    public string Fingerprint { get; }

    /// <summary>
    /// Optional human-readable representation.
    /// </summary>
    public string? TextRepresentation { get; }

    /// <summary>
    /// Encodes the memo bytes as base64 for Horizon queries.
    /// </summary>
    public string ToBase64() => Convert.ToBase64String(Value.Span);

    /// <summary>
    /// Determines whether the supplied memo bytes match the expected payload.
    /// Accepts Horizon padding behaviour for memo hash payloads.
    /// </summary>
    public bool Matches(ReadOnlySpan<byte> other)
    {
        var expected = Value.Span;
        if (expected.Length == other.Length)
        {
            return expected.SequenceEqual(other);
        }

        if (other.Length > expected.Length)
        {
            if (!other[..expected.Length].SequenceEqual(expected))
            {
                return false;
            }

            for (var i = expected.Length; i < other.Length; i++)
            {
                if (other[i] != 0)
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }
}
