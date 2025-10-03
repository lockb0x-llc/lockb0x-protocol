using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lockb0x.Core.Models;

/// <summary>
/// Represents the canonical Codex Entry data structure defined by the Lockb0x specification.
/// The model intentionally mirrors the JSON schema to guarantee predictable serialisation behaviour
/// for canonicalisation and signing flows.
/// </summary>
public sealed class CodexEntry
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("previous_id")]
    public string? PreviousId { get; init; }

    [JsonPropertyName("version")]
    public required string Version { get; init; }

    [JsonPropertyName("storage")]
    public required StorageDescriptor Storage { get; init; }

    [JsonPropertyName("encryption")]
    public EncryptionDescriptor? Encryption { get; init; }

    [JsonPropertyName("identity")]
    public required IdentityDescriptor Identity { get; init; }

    [JsonPropertyName("timestamp")]
    public required DateTimeOffset Timestamp { get; init; }

    [JsonPropertyName("anchor")]
    public required AnchorProof Anchor { get; init; }


    [JsonPropertyName("signature")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Signature { get; init; }

    // Optional: Ancillary signatures for multi-sig, endorsements, etc.
    [JsonPropertyName("signatures")]
    public required IReadOnlyList<SignatureProof> Signatures { get; init; }

    [JsonPropertyName("extensions")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonDocument? Extensions { get; init; }

    /// <summary>
    /// Creates a mutable builder for constructing a validated <see cref="CodexEntry"/>.
    /// </summary>
    public static CodexEntryBuilder Builder() => new();
}

public sealed class StorageDescriptor
{
    [JsonPropertyName("protocol")]
    public required string Protocol { get; init; }

    [JsonPropertyName("integrity_proof")]
    public required string IntegrityProof { get; init; }

    [JsonPropertyName("media_type")]
    public required string MediaType { get; init; }

    [JsonPropertyName("size_bytes")]
    public required long SizeBytes { get; init; }

    [JsonPropertyName("location")]
    public required StorageLocation Location { get; init; }
}

public sealed class StorageLocation
{
    [JsonPropertyName("region")]
    public required string Region { get; init; }

    [JsonPropertyName("jurisdiction")]
    public required string Jurisdiction { get; init; }

    [JsonPropertyName("provider")]
    public required string Provider { get; init; }
}

public sealed class EncryptionDescriptor
{
    [JsonPropertyName("algorithm")]
    public required string Algorithm { get; init; }

    [JsonPropertyName("key_ownership")]
    public required string KeyOwnership { get; init; }

    [JsonPropertyName("policy")]
    public EncryptionPolicy? Policy { get; init; }

    [JsonPropertyName("public_keys")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? PublicKeys { get; init; }

    [JsonPropertyName("last_controlled_by")]
    public required IReadOnlyList<string> LastControlledBy { get; init; }
}

public sealed class EncryptionPolicy
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    [JsonPropertyName("threshold")]
    public int? Threshold { get; init; }

    [JsonPropertyName("total")]
    public int? Total { get; init; }
}

public sealed class IdentityDescriptor
{
    [JsonPropertyName("org")]
    public required string Org { get; init; }

    [JsonPropertyName("process")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Process { get; init; }

    [JsonPropertyName("artifact")]
    public required string Artifact { get; init; }

    [JsonPropertyName("subject")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Subject { get; init; }
}

public sealed class AnchorProof
{
    [JsonPropertyName("chain")]
    public required string Chain { get; init; }

    [JsonPropertyName("tx_hash")]
    public required string TransactionHash { get; init; }

    [JsonPropertyName("hash_alg")]
    public required string HashAlgorithm { get; init; }

    [JsonPropertyName("token_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TokenId { get; init; }

    [JsonPropertyName("anchored_at")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTimeOffset? AnchoredAt { get; init; }
}

public sealed class SignatureProtectedHeader
{
    [JsonPropertyName("alg")]
    public required string Algorithm { get; init; }

    [JsonPropertyName("kid")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? KeyId { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalParameters { get; init; }
}

public sealed class SignatureProof
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = "internal"; // "internal", "zkp", "endorsement", etc.

    [JsonPropertyName("protected")]
    public required SignatureProtectedHeader ProtectedHeader { get; init; }

    [JsonPropertyName("signature")]
    public required string Signature { get; init; }
}

/// <summary>
/// Builder enforcing mandatory properties before constructing a Codex Entry instance.
/// </summary>
public sealed class CodexEntryBuilder
{
    private string? _id;
    private string? _previousId;
    private string? _version;
    private StorageDescriptor? _storage;
    private EncryptionDescriptor? _encryption;
    private IdentityDescriptor? _identity;
    private DateTimeOffset? _timestamp;
    private AnchorProof? _anchor;
    private IReadOnlyList<SignatureProof>? _signatures;
    private JsonDocument? _extensions;

    public CodexEntryBuilder WithId(Guid id)
    {
        _id = id.ToString();
        return this;
    }

    public CodexEntryBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public CodexEntryBuilder WithPreviousId(Guid? previousId)
    {
        _previousId = previousId?.ToString();
        return this;
    }

    public CodexEntryBuilder WithPreviousId(string? previousId)
    {
        _previousId = previousId;
        return this;
    }

    public CodexEntryBuilder WithVersion(string version)
    {
        _version = version;
        return this;
    }

    public CodexEntryBuilder WithStorage(StorageDescriptor storage)
    {
        _storage = storage;
        return this;
    }

    public CodexEntryBuilder WithEncryption(EncryptionDescriptor? encryption)
    {
        _encryption = encryption;
        return this;
    }

    public CodexEntryBuilder WithIdentity(IdentityDescriptor identity)
    {
        _identity = identity;
        return this;
    }

    public CodexEntryBuilder WithTimestamp(DateTimeOffset timestamp)
    {
        _timestamp = timestamp;
        return this;
    }

    public CodexEntryBuilder WithAnchor(AnchorProof anchor)
    {
        _anchor = anchor;
        return this;
    }

    public CodexEntryBuilder WithSignatures(IEnumerable<SignatureProof> signatures)
    {
        _signatures = signatures?.ToArray();
        return this;
    }

    public CodexEntryBuilder WithExtensions(JsonDocument? extensions)
    {
        _extensions = extensions;
        return this;
    }

    public CodexEntry Build()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(_id);
        ArgumentException.ThrowIfNullOrWhiteSpace(_version);
        if (_storage is null) throw new InvalidOperationException("Storage descriptor is required");
        if (_identity is null) throw new InvalidOperationException("Identity descriptor is required");
        if (_anchor is null) throw new InvalidOperationException("Anchor proof is required");
        if (_timestamp is null) throw new InvalidOperationException("Timestamp is required");
        if (_signatures is null || _signatures.Count == 0) throw new InvalidOperationException("At least one signature is required");

        return new CodexEntry
        {
            Id = _id!,
            PreviousId = _previousId,
            Version = _version!,
            Storage = _storage!,
            Encryption = _encryption,
            Identity = _identity!,
            Timestamp = _timestamp.Value,
            Anchor = _anchor!,
            Signatures = _signatures,
            Extensions = _extensions
        };
    }
}
