using System.Security.Cryptography;

namespace Lockb0x.Core.Canonicalization;

/// <summary>
/// Produces RFC 8785 JSON Canonicalization Scheme (JCS) outputs suitable for signing and hashing.
/// </summary>
public interface IJsonCanonicalizer
{
    /// <summary>
    /// Canonicalises the supplied payload into an RFC 8785 compliant UTF-8 JSON string.
    /// </summary>
    string Canonicalize<T>(T payload);

    /// <summary>
    /// Canonicalises and hashes the payload using the supplied algorithm.
    /// </summary>
    byte[] Hash<T>(T payload, HashAlgorithmName algorithm);
}
