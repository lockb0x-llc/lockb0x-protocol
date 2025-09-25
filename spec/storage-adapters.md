<file name=0 path=/Users/steven/Code/lockb0x-protocol/spec/storage-adapters.md># 4. Storage Adapters (Normative)

The Lockb0x Protocol is **storage-agnostic**.  
To support this, implementations MUST provide *Storage Adapters* that generate proofs for different backends.  
Adapters are responsible for extracting metadata, calculating checksums, and asserting jurisdiction/location claims.

---

## 4.1 Adapter Requirements

A Storage Adapter MUST implement at minimum:

- `get_integrity_proof(file): string`
  Returns the canonical [RFC 6920] `ni` URI for the file. When a backend exposes an alternate hash (e.g., CID, ETag), the adapter MUST transform it into the ni representation.

- `get_location_metadata(file): object`  
  Returns metadata including region, jurisdiction, and provider.  

- `get_size(file): integer`  
  Returns the file size in bytes.  

Adapters MAY also implement:

- `notarize(file): object` — produce a signed attestation from the provider or custodian.  
- `fetch(file_id): file` — retrieve the file from the backend for verification.

---

## 4.2 Supported Backends

The reference implementation MUST include at least three adapters:

- **IPFS**:
  - Proof: RFC 6920 ni-URI derived from the CID using canonicalization rules in Section 4.3.1.
  - The adapter MAY also expose the native `ipfs://<cid>` identifier for compatibility with IPFS tooling, but the canonical `storage.integrity_proof` MUST be the ni-URI.
  - Location: IPFS gateway region or pinning service jurisdiction.

- **S3-Compatible (AWS, MinIO, etc.)**:
  - Proof: The adapter MUST convert the ETag checksum into an ni-URI using canonicalization rules in Section 4.3.1.
  - Location: `region`, `jurisdiction`, and provider (`AWS`, `MinIO`).
  - Media type and size MUST be provided.

- **Google Cloud Storage (GCS)**:
  - Proof: The adapter MUST compute a SHA-256 digest of the object and express it as `ni:///sha-256;<digest>`.
  - The adapter MUST capture the canonical `crc32c` checksum returned by GCS metadata when available.
  - Location: MUST report the bucket location (e.g., `us-central1`), the governing jurisdiction (e.g., `US/CA`), and set `provider` to `Google Cloud`.
  - Media type and size MUST be provided.

- **Azure Blob Storage**:  
  - Proof: Adapter MUST compute a SHA-256 digest as canonical proof (`ni:///sha-256;…`). MD5/CRC64 MAY be recorded if supplied by Azure metadata, but MUST NOT be used as the canonical proof.  
  - For Stellar anchoring, an MD5 digest MAY be embedded in the transaction memo field when combined with the signing account’s public key, serving as a compact identifier within size constraints.  
  - Location: Azure region and legal jurisdiction.  

- **FTP/SFTP**:  
  - Proof: SHA-256 checksum.  
  - Location: server jurisdiction metadata (declared or notarized).  

- **Local/On-Prem Storage**:  
  - Proof: SHA-256 checksum.  
  - Location: MUST specify controlling entity and physical jurisdiction.  

- **Solid (gSolid / SolidProject.org)**:  
  - Proof: Since Solid Pods expose resources via HTTPS without standardized checksums, the adapter MUST download the resource content and compute a SHA-256 digest, expressing it as an RFC 6920 ni URI (`ni:///sha-256;<digest>`).  
  - Identity Binding: Solid uses WebID for identity and access control via WAC (Web Access Control) or ACP (Access Control Policies), enabling strong identity binding and permission management.  
  - Provenance: The adapter SHOULD integrate provenance metadata expressed in RDF and Linked Data formats with Lockb0x Codex Entries to enhance traceability and context.  
  - Location: MUST include jurisdiction and provider information based on the Solid Pod hosting environment and relevant legal controls.  
  - Using Solid with Lockb0x enables data sovereignty, aligns with EU data protection initiatives, and promotes interoperability through decentralized, user-controlled data storage.

---

## 4.3 Proof Formats

- Integrity proofs MUST use [RFC 6920] `ni` URIs as the canonical representation stored in `storage.integrity_proof`.  
- Provider-specific identifiers (e.g., S3 ETags, IPFS CIDs) are permitted as inputs to adapters but MUST be canonically mapped to ni-URIs before inclusion in a Codex Entry.
- For Stellar anchors only, shortened digests such as MD5 MAY be used in memo fields for compactness, but MUST always correspond to the canonical SHA-256 proof recorded in the Codex Entry.
- Proofs MUST be collision-resistant and reproducible across verification attempts.

### 4.3.1 Canonicalization Rules

The following rules define how provider-specific identifiers MUST be mapped to canonical ni-URIs:

**S3 ETag → ni-URI:**
- S3 ETags are typically MD5 digests for single-part uploads (objects ≤ 5GB).  
- For multipart uploads, S3 ETags contain a dash (e.g., `d41d8cd98f00b204e9800998ecf8427e-1`).  
- **Rule:** If ETag is a 32-character hex string without dash, interpret as MD5 digest:  
  `ni:///md5;<base64url-encode(hex-decode(etag))>`  
- **Rule:** If ETag contains dash or is not recognizable as MD5, adapter MUST download file content and compute SHA-256:  
  `ni:///sha-256;<base64url-encode(sha256(file_content))>`  

**IPFS CID → ni-URI:**
- IPFS CIDv0 (starting with `Qm`) uses SHA-256 multihash.  
- IPFS CIDv1 can use various hash functions (inspect multihash).  
- **Rule:** Extract the hash from CID multihash and verify it is SHA-256 (code 0x12):  
  `ni:///sha-256;<base64url-encode(extracted_sha256_bytes)>`  
- **Rule:** If CID uses non-SHA-256 hash, adapter MUST download file content and compute SHA-256.  

**Azure Blob MD5/CRC64 → ni-URI:**
- Azure may provide Content-MD5 headers for blobs.  
- **Rule:** If MD5 is available, use: `ni:///md5;<base64url-encode(md5_bytes)>`  
- **Rule:** If no usable hash metadata, download content and compute SHA-256.  

**GCS CRC32C/MD5 → ni-URI:**
- Google Cloud Storage provides crc32c and sometimes md5Hash metadata.  
- **Rule:** Prefer MD5 if available: `ni:///md5;<base64url-encode(md5_bytes)>`  
- **Rule:** If only CRC32C available, download content and compute SHA-256.  

All canonicalization MUST produce deterministic results across multiple adapter invocations for the same file.

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
[RFC 6838]: https://www.rfc-editor.org/rfc/rfc6838</file>