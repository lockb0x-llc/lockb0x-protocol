using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;

namespace Lockb0x.Storage.Internal;

internal static class CidUtility
{
    private const string Base32Alphabet = "abcdefghijklmnopqrstuvwxyz234567";

    public const int Sha2_256Code = 0x12;
    private const int DagPbCodec = 0x70;

    public static CidInfo Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("CID value cannot be null or empty.", nameof(value));
        }

        var trimmed = value.Trim();
        if (trimmed.StartsWith("ipfs://", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed[7..];
        }

        if (trimmed.StartsWith("/ipfs/", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed[6..];
        }

        var slashIndex = trimmed.IndexOf('/', StringComparison.Ordinal);
        if (slashIndex > 0)
        {
            trimmed = trimmed[..slashIndex];
        }

        byte[] raw;
        int version;
        int codec;
        ReadOnlySpan<byte> multihashBytes;

        if (trimmed.StartsWith("Qm", StringComparison.Ordinal))
        {
            raw = Base58Decode(trimmed);
            version = 0;
            codec = DagPbCodec;
            multihashBytes = raw;
        }
        else
        {
            raw = DecodeMultibase(trimmed, out _);
            var offset = 0;
            version = (int)ReadVarint(raw, ref offset);
            codec = (int)ReadVarint(raw, ref offset);
            multihashBytes = raw[offset..];
        }

        var (hashCode, digest) = ParseMultihash(multihashBytes);
        return new CidInfo(version, codec, hashCode, digest);
    }

    private static (int HashCode, byte[] Digest) ParseMultihash(ReadOnlySpan<byte> multihash)
    {
        if (multihash.IsEmpty)
        {
            throw new FormatException("Multihash payload is empty.");
        }

        var offset = 0;
        var code = (int)ReadVarint(multihash, ref offset);
        var length = (int)ReadVarint(multihash, ref offset);
        if (length < 0 || offset + length > multihash.Length)
        {
            throw new FormatException("Multihash length is invalid.");
        }

        var digest = multihash.Slice(offset, length).ToArray();
        return (code, digest);
    }

    private static byte[] DecodeMultibase(string value, out char prefix)
    {
        if (value.Length < 2)
        {
            throw new FormatException("CID is too short to contain multibase payload.");
        }

        prefix = value[0];
        var payload = value[1..];
        return prefix switch
        {
            'b' or 'B' => Base32Decode(payload),
            'z' or 'Z' => Base58Decode(payload),
            'f' or 'F' => Convert.FromHexString(payload),
            _ => throw new FormatException(string.Create(CultureInfo.InvariantCulture, $"Unsupported multibase prefix '{prefix}'."))
        };
    }

    private static byte[] Base32Decode(string value)
    {
        if (value.Length == 0)
        {
            return Array.Empty<byte>();
        }

        var sanitized = value.Replace("=", string.Empty, StringComparison.Ordinal).ToLowerInvariant();
        var buffer = 0;
        var bits = 0;
        var output = new List<byte>(sanitized.Length * 5 / 8);

        foreach (var c in sanitized)
        {
            var index = Base32Alphabet.IndexOf(c);
            if (index < 0)
            {
                throw new FormatException(string.Create(CultureInfo.InvariantCulture, $"Invalid base32 character '{c}'."));
            }

            buffer = (buffer << 5) | index;
            bits += 5;
            if (bits >= 8)
            {
                bits -= 8;
                output.Add((byte)((buffer >> bits) & 0xFF));
            }
        }

        if (bits > 0 && (buffer & ((1 << bits) - 1)) != 0)
        {
            throw new FormatException("Non-zero padding bits encountered in base32 payload.");
        }

        return output.ToArray();
    }

    private static byte[] Base58Decode(string value)
    {
        const string alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
        if (value.Length == 0)
        {
            return Array.Empty<byte>();
        }

        var result = BigInteger.Zero;
        foreach (var c in value)
        {
            var index = alphabet.IndexOf(c);
            if (index < 0)
            {
                throw new FormatException(string.Create(CultureInfo.InvariantCulture, $"Invalid base58 character '{c}'."));
            }

            result = result * 58 + index;
        }

        var bytes = result.ToByteArray(isUnsigned: true, isBigEndian: true);
        var leadingZeroCount = 0;
        foreach (var c in value)
        {
            if (c == '1')
            {
                leadingZeroCount++;
                continue;
            }
            break;
        }

        if (leadingZeroCount > 0)
        {
            var extended = new byte[leadingZeroCount + bytes.Length];
            Array.Copy(bytes, 0, extended, leadingZeroCount, bytes.Length);
            bytes = extended;
        }

        return bytes;
    }

    private static ulong ReadVarint(ReadOnlySpan<byte> data, ref int offset)
    {
        ulong result = 0;
        var shift = 0;
        while (offset < data.Length)
        {
            var b = data[offset++];
            result |= (ulong)(b & 0x7F) << shift;
            if ((b & 0x80) == 0)
            {
                return result;
            }

            shift += 7;
            if (shift > 63)
            {
                throw new FormatException("Varint exceeds 64-bit range.");
            }
        }

        throw new FormatException("Unexpected end of buffer while decoding varint.");
    }
}

internal readonly record struct CidInfo(int Version, int Codec, int HashAlgorithmCode, byte[] Digest)
{
    public bool UsesSha256 => HashAlgorithmCode == CidUtility.Sha2_256Code && Digest.Length == 32;
}
