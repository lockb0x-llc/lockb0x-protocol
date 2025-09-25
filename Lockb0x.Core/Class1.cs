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

	/// <summary>
	/// Validates a JSON string against the Codex Entry schema with strict additionalProperties=false
	/// </summary>
	public static bool ValidateJson(string jsonString, out List<string> errors)
	{
		errors = new List<string>();
		
		try
		{
			// Parse JSON to check for unknown fields
			using var document = System.Text.Json.JsonDocument.Parse(jsonString);
			var root = document.RootElement;
			
			// Check for unknown properties at root level
			var allowedRootProperties = new HashSet<string>
			{
				"id", "previous_id", "version", "storage", "encryption", "identity", 
				"timestamp", "anchor", "signatures", "extensions"
			};
			
			foreach (var property in root.EnumerateObject())
			{
				if (!allowedRootProperties.Contains(property.Name))
				{
					errors.Add($"Unknown field '{property.Name}' is not allowed in Codex Entry");
				}
			}
			
			// Check nested objects for unknown fields
			if (root.TryGetProperty("storage", out var storage))
			{
				ValidateObjectProperties(storage, new HashSet<string>
				{
					"protocol", "integrity_proof", "media_type", "size_bytes", "location"
				}, "storage", errors);
				
				if (storage.TryGetProperty("location", out var location))
				{
					ValidateObjectProperties(location, new HashSet<string>
					{
						"region", "jurisdiction", "provider"
					}, "storage.location", errors);
				}
			}
			
			if (root.TryGetProperty("encryption", out var encryption))
			{
				ValidateObjectProperties(encryption, new HashSet<string>
				{
					"algorithm", "key_ownership", "policy", "public_keys", "last_controlled_by"
				}, "encryption", errors);
				
				if (encryption.TryGetProperty("policy", out var policy))
				{
					ValidateObjectProperties(policy, new HashSet<string>
					{
						"type", "threshold", "total"
					}, "encryption.policy", errors);
				}
			}
			
			if (root.TryGetProperty("identity", out var identity))
			{
				ValidateObjectProperties(identity, new HashSet<string>
				{
					"org", "process", "artifact", "subject"
				}, "identity", errors);
			}
			
			if (root.TryGetProperty("anchor", out var anchor))
			{
				ValidateObjectProperties(anchor, new HashSet<string>
				{
					"chain", "tx_hash", "hash_alg", "token_id"
				}, "anchor", errors);
			}
			
			if (root.TryGetProperty("signatures", out var signatures))
			{
				if (signatures.ValueKind == System.Text.Json.JsonValueKind.Array)
				{
					int index = 0;
					foreach (var signature in signatures.EnumerateArray())
					{
						ValidateObjectProperties(signature, new HashSet<string>
						{
							"protected", "signature"
						}, $"signatures[{index}]", errors);
						
						if (signature.TryGetProperty("protected", out var protectedObj))
						{
							ValidateObjectProperties(protectedObj, new HashSet<string>
							{
								"alg", "kid"
							}, $"signatures[{index}].protected", errors);
						}
						index++;
					}
				}
			}
			
			return errors.Count == 0;
		}
		catch (System.Text.Json.JsonException ex)
		{
			errors.Add($"Invalid JSON: {ex.Message}");
			return false;
		}
	}

	/// <summary>
	/// Validates Stellar-specific constraints for anchor proofs
	/// </summary>
	public static bool ValidateStellarAnchor(AnchorProof anchor, out List<string> errors)
	{
		errors = new List<string>();
		
		if (anchor == null)
		{
			errors.Add("AnchorProof cannot be null");
			return false;
		}
		
		// Check if this is a Stellar chain
		if (anchor.ChainId?.StartsWith("stellar:") == true)
		{
			// For Stellar anchors, the memo field is constrained to 28 bytes
			// This method can be extended to validate memo content when available
			
			// Ensure hash algorithm is supported
			if (string.IsNullOrEmpty(anchor.HashAlgorithm))
			{
				errors.Add("Stellar anchors must specify a hash_alg field");
			}
			else if (anchor.HashAlgorithm.ToLowerInvariant() != "sha-256")
			{
				errors.Add($"Stellar anchors must use SHA-256 hash algorithm, but got '{anchor.HashAlgorithm}'");
			}
			
			// Validate transaction hash format
			if (string.IsNullOrEmpty(anchor.TxHash))
			{
				errors.Add("Stellar anchors must have a valid tx_hash");
			}
			
			// Additional Stellar-specific validations can be added here
			// For example, validating that the memo was properly constructed with MD5+publickey
		}
		
		return errors.Count == 0;
	}

	private static void ValidateObjectProperties(System.Text.Json.JsonElement element, HashSet<string> allowedProperties, string path, List<string> errors)
	{
		if (element.ValueKind != System.Text.Json.JsonValueKind.Object) return;
		
		foreach (var property in element.EnumerateObject())
		{
			if (!allowedProperties.Contains(property.Name))
			{
				errors.Add($"Unknown field '{property.Name}' is not allowed in {path}");
			}
		}
	}
}
