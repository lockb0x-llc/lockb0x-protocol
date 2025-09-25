# Appendix B. Codex Entry JSON Schema (Non-Normative)

This appendix provides a full JSON Schema definition for Codex Entries.  
It is intended as a reference for validation and interoperability.  
The schema reflects all required and optional fields described in Section 3 (Data Model).

**Note:** The `encryption` object is intentionally NOT included in the  
root-level `required` array, making it optional in compliance with the protocol  
specification that allows plaintext assets.

**Note on Additional Properties:** This schema explicitly sets `"additionalProperties": false` on all object types to ensure strict validation. Verifiers MUST reject Codex Entries containing any fields not defined in this schema. This approach ensures forward compatibility while maintaining cryptographic security by preventing injection of unexpected metadata that could compromise verification processes.

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "title": "Lockb0x Codex Entry",
  "type": "object",
  "additionalProperties": false,
  "required": [
    "id",
    "version",
    "storage",
    "identity",
    "timestamp",
    "anchor",
    "signatures"
  ],
  "properties": {
    "id": {
      "type": "string",
      "format": "uuid",
      "description": "Unique identifier (UUID v4)."
    },
    "previous_id": {
      "type": "string",
      "format": "uuid",
      "description": "UUID of the immediate prior Codex Entry; omitted for the first entry."
    },
    "version": {
      "type": "string",
      "pattern": "^[0-9]+\\.[0-9]+\\.[0-9]+$",
      "description": "Protocol version string (e.g., 1.0.0)."
    },
    "storage": {
      "type": "object",
      "additionalProperties": false,
      "required": ["protocol", "integrity_proof", "media_type", "size_bytes", "location"],
      "properties": {
        "protocol": {
          "type": "string",
          "enum": ["ipfs", "s3", "azureblob", "gcs", "ftp", "local"],
          "description": "Storage backend protocol (IPFS, S3, Azure Blob, Google Cloud Storage, FTP, Local)."
        },
        "integrity_proof": {
          "type": "string",
          "pattern": "^ni:///[a-z0-9\\-]+;[A-Za-z0-9\\-_]+$",
          "description": "Integrity proof expressed as a canonical RFC 6920 ni-URI (REQUIRED). Provider-specific identifiers (e.g., S3 ETags, IPFS CIDs) MUST be canonically mapped to ni-URIs before inclusion."
        },
        "media_type": {
          "type": "string",
          "description": "IANA MIME type (RFC 6838)."
        },
        "size_bytes": {
          "type": "integer",
          "minimum": 0,
          "description": "Exact file size in bytes."
        },
        "location": {
          "type": "object",
          "additionalProperties": false,
          "required": ["region", "jurisdiction", "provider"],
          "properties": {
            "region": {"type": "string"},
            "jurisdiction": {"type": "string"},
            "provider": {"type": "string"}
          }
        }
      }
    },
    "encryption": {
      "type": "object",
      "additionalProperties": false,
      "required": ["algorithm", "key_ownership"],
      "properties": {
        "algorithm": {
          "type": "string",
          "enum": ["AES-256-GCM", "ChaCha20-Poly1305"],
          "description": "Encryption algorithm used."
        },
        "key_ownership": {
          "type": "string",
          "enum": ["org-managed", "multi-sig", "custodian"],
          "description": "Key ownership model."
        },
        "policy": {
          "type": "object",
          "additionalProperties": false,
          "required": ["type"],
          "properties": {
            "type": {"type": "string", "enum": ["threshold"]},
            "threshold": {"type": "integer", "minimum": 1},
            "total": {"type": "integer", "minimum": 1}
          }
        },
        "public_keys": {
          "type": "array",
          "items": {"type": "string"},
          "description": "Public keys participating in multi-sig or custody."
        },
        "last_controlled_by": {
          "type": "array",
          "items": {"type": "string"},
          "description": "Keys that executed the last control event; required when key_ownership is 'multi-sig', MAY be single for 'org-managed', and omitted if no encryption."
        }
      },
      "allOf": [
        {
          "if": {
            "properties": {"key_ownership": {"const": "multi-sig"}}
          },
          "then": {
            "required": ["last_controlled_by"]
          }
        }
      ]
    },
    "identity": {
      "type": "object",
      "additionalProperties": false,
      "required": ["org", "process", "artifact"],
      "properties": {
        "org": {
          "type": "string",
          "description": "Root organizational DID or account."
        },
        "process": {
          "type": "string",
          "description": "Process- or activity-level DID anchoring provenance."
        },
        "artifact": {
          "type": "string",
          "description": "Artifact identifier (e.g., work order, case ID, document ID)."
        },
        "subject": {
          "type": "string",
          "description": "Optional DID referencing the individual, entity, or asset that is the subject of the entry."
        }
      }
    },
    "timestamp": {
      "type": "string",
      "format": "date-time",
      "description": "UTC ISO 8601 timestamp."
    },
    "anchor": {
      "type": "object",
      "additionalProperties": false,
      "required": ["chain", "tx_hash", "hash_alg"],
      "properties": {
        "chain": {"type": "string", "description": "CAIP-2 blockchain identifier."},
        "tx_hash": {"type": "string", "description": "Transaction hash referencing the anchor."},
        "hash_alg": {"type": "string", "enum": ["SHA256", "SHA3-256"]},
        "token_id": {
          "type": "string",
          "pattern": "^(0x[a-fA-F0-9]+|[A-Za-z0-9][A-Za-z0-9._:-]*)$",
          "description": "Optional NFT identifier; accepts 0x-prefixed hex or alphanumeric strings with separators (:, -, _, .)."
        }
      }
    },
    "signatures": {
      "type": "array",
      "minItems": 1,
      "items": {
        "type": "object",
        "additionalProperties": false,
        "required": ["protected", "signature"],
        "properties": {
          "protected": {
            "type": "object",
            "additionalProperties": false,
            "required": ["alg", "kid"],
            "properties": {
              "alg": {"type": "string", "description": "Algorithm identifier (e.g., EdDSA)."},
              "kid": {"type": "string", "description": "Key identifier."}
            }
          },
          "signature": {"type": "string", "description": "Base64URL-encoded signature value."}
        }
      },
      "description": "Signatures MUST be generated after anchoring, covering the full Codex Entry including the anchor reference."
    },
    "extensions": {
      "type": "object",
      "description": "Optional metadata extensions, MAY follow JSON-LD."
    }
  }
}
```
