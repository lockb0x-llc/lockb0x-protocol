using System;
using System.Security.Cryptography;
using System.Text;

namespace Lockb0x.Core.Utilities;

/// <summary>
/// Helper functions for working with RFC 6920 Named Information URIs (ni-URIs).
/// </summary>
public static class NiUri
{
    public static string Create(ReadOnlySpan<byte> digest, string algorithm = "sha-256")
    {
        if (digest.IsEmpty) throw new ArgumentException("Digest cannot be empty", nameof(digest));
        var normalizedAlgorithm = algorithm.ToLowerInvariant();
        var b64 = Convert.ToBase64String(digest);
        Span<char> buffer = stackalloc char[b64.Length];
        int len = Base64UrlEncode(b64, buffer);
        return $"ni:///{normalizedAlgorithm};{new string(buffer[..len])}";
    }

    public static string Create(ReadOnlySpan<byte> content, HashAlgorithmName algorithm)
    {
        using var hash = IncrementalHash.CreateHash(algorithm);
        hash.AppendData(content);
        return Create(hash.GetHashAndReset(), algorithm.Name.ToLowerInvariant());
    }

    public static bool TryParse(string? value, out string algorithm, out byte[] digest)
    {
        algorithm = string.Empty;
        digest = Array.Empty<byte>();
        if (string.IsNullOrWhiteSpace(value)) return false;
        if (!value.StartsWith("ni:///", StringComparison.Ordinal)) return false;
        var payload = value[6..];
        var parts = payload.Split(';', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2) return false;
        algorithm = parts[0].ToLowerInvariant();
        try
        {
            digest = FromBase64Url(parts[1]);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static int Base64UrlEncode(ReadOnlySpan<char> base64, Span<char> destination)
    {
        int index = 0;
        foreach (var c in base64)
        {
            if (c == '=')
            {
                continue;
            }

            destination[index++] = c switch
            {
                '+' => '-',
                '/' => '_',
                _ => c
            };
        }
        return index;
    }

    private static byte[] FromBase64Url(string value)
    {
        var sb = new StringBuilder(value.Length);
        foreach (var c in value)
        {
            sb.Append(c switch
            {
                '-' => '+',
                '_' => '/',
                _ => c
            });
        }

        var padded = sb.ToString();
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }

        return Convert.FromBase64String(padded);
    }
}
