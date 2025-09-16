using System.Text.Json.Serialization;
using System.Collections.Generic;
using System;

namespace Lockb0x.Core;

/// <summary>
/// Represents a Codex Entry as defined by the Lockb0x Protocol.
/// </summary>
public class CodexEntry
{
	[JsonPropertyName("id")]
	public string Id { get; set; } = string.Empty;

	[JsonPropertyName("storage")]
	public List<StorageProof> Storage { get; set; } = new();

	[JsonPropertyName("integrity")]
	public List<IntegrityProof> Integrity { get; set; } = new();

	[JsonPropertyName("signatures")]
	public List<SignatureProof> Signatures { get; set; } = new();

	[JsonPropertyName("anchors")]
	public List<AnchorProof> Anchors { get; set; } = new();

	[JsonPropertyName("provenance")]
	public ProvenanceMetadata Provenance { get; set; } = new();

	[JsonPropertyName("revision")]
	public string? Revision { get; set; }

	[JsonPropertyName("timestamp")]
	public DateTimeOffset Timestamp { get; set; }
}

public class StorageProof
{
	public string Location { get; set; } = string.Empty; // ni-URI or storage URI
	public string? Adapter { get; set; }
	public string? Jurisdiction { get; set; }
	public long? Size { get; set; }
	public string? MediaType { get; set; }
}

public class IntegrityProof
{
	public string Algorithm { get; set; } = string.Empty;
	public string Hash { get; set; } = string.Empty;
}

public class SignatureProof
{
	public string Algorithm { get; set; } = string.Empty;
	public string Value { get; set; } = string.Empty;
	public string Signer { get; set; } = string.Empty;
	public string? KeyId { get; set; }
}

public class AnchorProof
{
	public string ChainId { get; set; } = string.Empty; // CAIP-2
	public string TxHash { get; set; } = string.Empty;
	public string HashAlgorithm { get; set; } = string.Empty;
	public DateTimeOffset? AnchoredAt { get; set; }
}

public class ProvenanceMetadata
{
	public string? WasGeneratedBy { get; set; }
	public string? WasAnchoredIn { get; set; }
	public List<string>? RevisionChain { get; set; }
}

// Placeholder for JCS canonicalization
public static class JsonCanonicalizer
{
	public static string Canonicalize(object obj)
	{
		// TODO: Implement RFC 8785 JCS canonicalization
		throw new NotImplementedException();
	}
}

// Placeholder for ni URI helpers
public static class NiUriHelper
{
	public static string ComputeNiUri(byte[] data, string algorithm = "sha-256")
	{
		// TODO: Implement RFC 6920 ni URI generation
		throw new NotImplementedException();
	}
}

// Placeholder for JSON schema validation
public static class CodexEntryValidator
{
	public static bool Validate(CodexEntry entry, out List<string> errors)
	{
		// TODO: Implement JSON schema validation
		errors = new List<string> { "Validation not implemented." };
		return false;
	}
}
