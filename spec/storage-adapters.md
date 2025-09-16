


# 4. Storage Adapters (Normative)

The Lockb0x Protocol is **storage-agnostic**.  
To support this, implementations MUST provide *Storage Adapters* that generate proofs for different backends.  
Adapters are responsible for extracting metadata, calculating checksums, and asserting jurisdiction/location claims.

---

## 4.1 Adapter Requirements

A Storage Adapter MUST implement at minimum:

- `get_integrity_proof(file): string`  
  Returns an [RFC 6920] `ni` URI or equivalent hash-based identifier of the file.  

- `get_location_metadata(file): object`  
  Returns metadata including region, jurisdiction, and provider.  

- `get_size(file): integer`  
  Returns the file size in bytes.  

Adapters MAY also implement:

- `notarize(file): object` — produce a signed attestation from the provider or custodian.  
- `fetch(file_id): file` — retrieve the file from the backend for verification.

---

## 4.2 Supported Backends

The reference implementation MUST include at least two adapters:  

- **IPFS**:  
  - Proof: CID (Content Identifier).  
  - Expressed as `ipfs://<cid>` or `ni:///sha-256;<digest>`.  
  - Location: IPFS gateway region or pinning service jurisdiction.  

- **S3-Compatible (AWS, MinIO, etc.)**:  
  - Proof: ETag checksum.  
  - Location: `region`, `jurisdiction`, and provider (`AWS`, `MinIO`).  
  - Media type and size MUST be provided.  

Implementations SHOULD also support:  

- **Azure Blob Storage**:  
  - Proof: MD5/CRC64 checksum.  
  - Location: Azure region and legal jurisdiction.  

- **FTP/SFTP**:  
  - Proof: SHA-256 checksum.  
  - Location: server jurisdiction metadata (declared or notarized).  

- **Local/On-Prem Storage**:  
  - Proof: SHA-256 checksum.  
  - Location: MUST specify controlling entity and physical jurisdiction.  

---

## 4.3 Proof Formats

- Integrity proofs MUST use [RFC 6920] `ni` URIs whenever possible.  
- Backends that provide native identifiers (e.g., IPFS CIDs, S3 ETags) MAY include them in addition to `ni` URIs.  
- Proofs MUST be collision-resistant and reproducible across verification attempts.  

---

## 4.4 Media Type and Size

- Each adapter MUST declare the file’s media type using [RFC 6838] IANA-registered MIME types.  
- Each adapter MUST declare the file size in bytes.  

---

## 4.5 Jurisdiction Metadata

- Each adapter MUST provide jurisdiction metadata, including:  
  - `region`: technical storage region (e.g., `us-east-1`).  
  - `jurisdiction`: legal sovereignty code (e.g., `US/NY`, `EU/DE`).  
  - `provider`: the service provider or controlling entity.  

- Where the backend cannot natively guarantee jurisdiction, the adapter MUST allow declaration or notarization of jurisdiction claims.

---

[RFC 6920]: https://www.rfc-editor.org/rfc/rfc6920
[RFC 6838]: https://www.rfc-editor.org/rfc/rfc6838