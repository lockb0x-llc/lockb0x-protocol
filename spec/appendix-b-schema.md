

# Appendix B. Codex Entry JSON Schema (Non-Normative)

This appendix provides a full JSON Schema definition for Codex Entries.  
It is intended as a reference for validation and interoperability.  
The schema reflects all required and optional fields described in Section 3 (Data Model).

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "title": "Lockb0x Codex Entry",
  "type": "object",
  "required": [
    "id",
    "version",
    "storage",
    "encryption",
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
      "required": ["protocol", "integrity_proof", "media_type", "size_bytes", "location"],
      "properties": {
        "protocol": {
          "type": "string",
          "enum": ["ipfs", "s3", "azureblob", "gcp", "ftp", "local"],
          "description": "Storage backend protocol."
        },
        "integrity_proof": {
          "type": "string",
          "pattern": "^ni:///sha-256;[A-Za-z0-9_-]+$",
          "description": "Integrity proof using RFC 6920 ni-URI."
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
      "required": ["algorithm", "key_ownership", "last_controlled_by"],
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
          "description": "Keys that executed the last control event. Required when encryption metadata is present."
        }
      }
    },
    "identity": {
      "type": "object",
      "required": ["org"],
      "properties": {
        "org": {"type": "string", "description": "Stellar account or W3C DID."},
        "project": {"type": "string", "description": "Optional project DID."},
        "context": {"type": "string", "description": "Optional context ID (e.g., work order)."}
      }
    },
    "timestamp": {
      "type": "string",
      "format": "date-time",
      "description": "UTC ISO 8601 timestamp."
    },
    "anchor": {
      "type": "object",
      "required": ["chain", "tx_hash", "hash_alg"],
      "properties": {
        "chain": {"type": "string", "description": "CAIP-2 blockchain identifier."},
        "tx_hash": {"type": "string", "description": "Transaction hash referencing the anchor."},
        "hash_alg": {"type": "string", "enum": ["SHA256", "SHA3-256"]}
      }
    },
    "signatures": {
      "type": "array",
      "minItems": 1,
      "items": {
        "type": "object",
        "required": ["protected", "signature"],
        "properties": {
          "protected": {
            "type": "object",
            "required": ["alg", "kid"],
            "properties": {
              "alg": {"type": "string", "description": "Algorithm identifier (e.g., EdDSA)."},
              "kid": {"type": "string", "description": "Key identifier."}
            }
          },
          "signature": {"type": "string", "description": "Base64URL-encoded signature value."}
        }
      }
    },
    "extensions": {
      "type": "object",
      "description": "Optional metadata extensions, MAY follow JSON-LD."
    }
  }
}
```

---