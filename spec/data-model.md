# 3. Data Model (Normative)

The Lockb0x Protocol defines a structured JSON object called a **Codex Entry**.  
This is the fundamental unit of the protocol, capturing proofs of integrity, storage, identity, and blockchain anchoring.

---

## 3.1 Codex Entry Schema

A Codex Entry MUST be expressed as JSON. The following example shows a  
comprehensive Codex Entry with all possible fields:

```json
{
  "id": "uuid-v4",
  "previous_id": "uuid-v4",
  "version": "1.0.0",
  "storage": {
    "protocol": "ipfs|s3|azureblob|gcs|ftp|local",
    "integrity_proof": "ni:///sha-256;<digest>",
    "media_type": "application/pdf",
    "size_bytes": 204800,
    "location": {
      "region": "us-central1",
      "jurisdiction": "US/CA",
      "provider": "Google Cloud"
    }
  },
  "encryption": {
    "algorithm": "AES-256-GCM",
    "key_ownership": "org-managed|multi-sig|custodian",
    "policy": {
      "type": "threshold",
      "threshold": 2,
      "total": 3
    },
    "public_keys": ["stellar:GA123...", "did:example:abc"],
    "last_controlled_by": ["stellar:GA123...", "stellar:GB456..."]
  },
  "identity": {
    "org": "did:example:org123",
    "process": "did:example:proj456",
    "artifact": "workorder-789",
    "subject": "did:example:asset999"
  },
  "timestamp": "2025-09-14T12:00:00Z",
  "anchor": {
    "chain": "stellar:pubnet",
    "tx_hash": "abcdef123456...",
    "hash_alg": "SHA256"
  },
  "signatures": [
    {
      "protected": {"alg": "EdDSA", "kid": "stellar:GA123..."},
      "signature": "base64url-edsig..."
    }
  ],
  "extensions": {}
}
```

**Note:** The above example shows a comprehensive entry with all possible fields.  
Many fields are optional, including the entire `encryption` object which MUST be  
omitted when assets are stored without encryption. See Section 3.2 for required  
fields and Section 3.3 for optional fields.

---

## 3.2 Required Fields

- `id` MUST be a UUID v4.  
- `version` MUST indicate the protocol version.  
- `storage.protocol` MUST declare the backend adapter.
- `storage.integrity_proof` MUST be expressed as a canonical [RFC 6920] Named Information URI (ni-URI) derived from the stored payload. Provider-specific identifiers (such as IPFS CIDs or S3 ETags) MUST be canonically mapped to ni-URIs by storage adapters before inclusion in the Codex Entry.
- `storage.media_type` MUST use [RFC 6838] IANA-registered MIME types.  
- `storage.size_bytes` MUST indicate the exact file size.  
- `storage.location` MUST specify region, jurisdiction, and provider.
- When the `encryption` object is present (i.e., when encryption is applied):
  - `encryption.algorithm` MUST declare the encryption scheme.
  - `encryption.key_ownership` MUST map to recognized KMS custodian roles.
  - `encryption.policy` MUST define the control model (e.g., threshold, m-of-n).
  - `encryption.last_controlled_by` MUST be present for multi-sig control models and MAY be a single key for org-managed models; it MUST list the key(s) that executed the last control event.
- The `encryption` object MUST be omitted entirely for plaintext assets stored without encryption.
- `identity.org` MUST be either a Stellar account or a W3C DID.
- `identity.process` MUST be a DID or account under the control of `identity.org` when provided.
- `identity.artifact` MUST be present for workflows, work orders, or provenance tracks and represents a stable identifier for the operational workflow in which the entry participates.
- `identity.subject` MAY reference the individual, entity, or asset that is the subject of the record; when present it MUST be expressed as a DID or account identifier.
- The identity hierarchy is: `org` → `process` → `artifact` → `subject`, where `artifact` is required for workflows/work orders, and `subject` is optional for entity/asset/person.
- `timestamp` MUST be UTC ISO 8601.  
- `anchor.chain` MUST use [CAIP-2] identifiers.  
- `anchor.tx_hash` MUST contain a blockchain transaction reference.  
- `signatures` MUST use [JOSE JWS] or [COSE Sign1] objects; signatures are created **after anchoring** and MUST cover the full Codex Entry including the anchor object.
- `previous_id` MUST link to the UUID of the immediate prior Codex Entry when the record is a revision; it MUST be omitted for the first entry in a chain.

The `identity` object forms a provenance hierarchy: `org` anchors the controlling organization, `process` narrows provenance to a program or initiative managed by that organization, and `artifact` binds the record to an operational workflow such as a work order or case identifier rooted in Pakana/UCC Article 12 practices. `subject` optionally identifies the individual, entity, or asset described by the entry without altering that hierarchy.

---

## 3.3 Optional Fields

- `encryption.public_keys` MAY list public keys for multi-sig or escrow.  
- Codex Entries MUST omit the entire `encryption` object when assets are stored without encryption. When the object is provided, it MUST satisfy the constraints defined in Section 3.2.
- `encryption.policy` MAY be omitted for single-key ownership but MUST be present for multi-sig control models.  
- `identity.process` MAY specify a sub-identity.  
- `identity.artifact` MAY link to a business or compliance context (e.g., work order, case ID, transaction).
- `identity.subject` MAY reference a DID for a person, organization, or asset that the Codex Entry concerns.
- `extensions` MAY use JSON-LD for additional metadata.
- `previous_id` MAY be omitted if the Codex Entry is the first version of an asset.

### 3.3.1 Encryption Metadata Examples

When encryption metadata is provided, it MUST include `last_controlled_by` as per the ownership model:

```json
{
  "encryption": {
    "algorithm": "AES-256-GCM",
    "key_ownership": "multi-sig",
    "last_controlled_by": ["stellar:GA123...", "stellar:GB456..."]
  }
}
```

For org-managed single key ownership, `last_controlled_by` MAY be a single key:

```json
{
  "encryption": {
    "algorithm": "AES-256-GCM",
    "key_ownership": "org-managed",
    "last_controlled_by": ["stellar:GA123..."]
  }
}
```

Codex Entries stored without encryption MUST omit the entire `encryption` object (and thus `last_controlled_by`):

```json
{
  "id": "uuid-v4",
  "version": "1.0.0",
  "storage": {
    "protocol": "ipfs",
    "integrity_proof": "ni:///sha-256;<digest>",
    "media_type": "application/pdf",
    "size_bytes": 204800,
    "location": {
      "region": "eu-west-1",
      "jurisdiction": "EU/DE",
      "provider": "AWS"
    }
  },
  "timestamp": "2025-09-14T12:00:00Z"
}
```

---

## 3.4 Key Ownership Semantics

`encryption.key_ownership` values MUST align with recognized key management roles:

- `org-managed` → Keys are held solely by the declaring organization.  
- `multi-sig` → Keys are held by multiple parties; operations require quorum.  
- `custodian` → Keys are held by a third-party custodian.  

Mappings SHOULD align with [NIST SP 800-57 Part 1] roles.

---

## 3.5 Revision Chains

Codex Entries MAY form a revision chain by referencing a `previous_id`.  
This creates an immutable, linked sequence of records.  
Verifiers MUST be able to traverse backward through `previous_id` links to reconstruct the full history of an asset or CER (Controllable Electronic Record).  
