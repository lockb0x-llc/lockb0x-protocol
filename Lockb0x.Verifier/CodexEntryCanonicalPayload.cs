using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Lockb0x.Core.Canonicalization;
using Lockb0x.Core.Models;

namespace Lockb0x.Verifier;

/// <summary>
/// Provides helpers for constructing canonical signing payloads for Codex Entries.
/// </summary>
public static class CodexEntryCanonicalPayload
{
    /// <summary>
    /// Creates the canonical UTF-8 payload representing a Codex Entry for signing and verification.
    /// Signatures intentionally exclude the <c>signatures</c> array to avoid circular dependencies.
    /// </summary>
    public static byte[] CreatePayload(IJsonCanonicalizer canonicalizer, CodexEntry entry)
    {
        ArgumentNullException.ThrowIfNull(canonicalizer);
        ArgumentNullException.ThrowIfNull(entry);

        var json = canonicalizer.Canonicalize(CreateSnapshot(entry));
        return Encoding.UTF8.GetBytes(json);
    }

    /// <summary>
    /// Computes a deterministic hash of the canonical signing payload using the supplied algorithm.
    /// </summary>
    public static byte[] CreateHash(IJsonCanonicalizer canonicalizer, CodexEntry entry, HashAlgorithmName algorithm)
    {
        ArgumentNullException.ThrowIfNull(canonicalizer);
        ArgumentNullException.ThrowIfNull(entry);

        return canonicalizer.Hash(CreateSnapshot(entry), algorithm);
    }

    internal static SignableCodexEntry CreateSnapshot(CodexEntry entry)
    {
        return SignableCodexEntry.FromEntry(entry);
    }

    internal sealed record SignableCodexEntry
    {
        [JsonPropertyName("id")]
        public required string Id { get; init; }

        [JsonPropertyName("previous_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? PreviousId { get; init; }

        [JsonPropertyName("version")]
        public required string Version { get; init; }

        [JsonPropertyName("storage")]
        public required StorageDescriptor Storage { get; init; }

        [JsonPropertyName("encryption")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public EncryptionDescriptor? Encryption { get; init; }

        [JsonPropertyName("identity")]
        public required IdentityDescriptor Identity { get; init; }

        [JsonPropertyName("timestamp")]
        public required DateTimeOffset Timestamp { get; init; }

        [JsonPropertyName("anchor")]
        public required AnchorProof Anchor { get; init; }

        [JsonPropertyName("extensions")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public JsonDocument? Extensions { get; init; }

        // signatures intentionally excluded from canonicalization

        public static SignableCodexEntry FromEntry(CodexEntry entry)
        {
            return new SignableCodexEntry
            {
                Id = entry.Id,
                PreviousId = entry.PreviousId,
                Version = entry.Version,
                Storage = entry.Storage,
                Encryption = entry.Encryption,
                Identity = entry.Identity,
                Timestamp = entry.Timestamp,
                Anchor = entry.Anchor,
                Extensions = entry.Extensions
            };
        }
    }
}
