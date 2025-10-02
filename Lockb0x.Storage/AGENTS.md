# Lockb0x.Storage — AGENTS.md

## Comprehensive Technical & Functional Plan: IPFS Storage Adapter Implementation

---

### 1. Overview

This document provides a detailed plan for implementing the IPFS storage adapter for the Lockb0x Protocol reference implementation. It is based on the protocol specification, referenced standards, and best practices for .NET development and decentralized storage.

---

### 2. Specification & Standards Alignment

- **Reference:** See `/spec/storage-adapters.md`, `/spec/appendix-a-flows.md`, `/spec/data-model.md`, RFC 6920 (ni-URI), and IPFS documentation.
- **Requirements:**
  - Store files in IPFS and retrieve the CID.
  - Compute canonical integrity proof as RFC 6920 ni-URI (SHA-256 of file content).
  - Return all required metadata for Codex Entry: protocol, integrity_proof, location, size, media type.
  - Support provenance, jurisdiction, and auditability.

---

### 3. Technical Design

#### a. Interface & Adapter Structure

- Implement `IStorageAdapter` (see existing interface in Lockb0x.Storage).
- Create `IpfsStorageAdapter` class with methods:
  - `Task<StorageDescriptor> StoreAsync(Stream file, string fileName, string mediaType, ...)`
  - `Task<Stream> RetrieveAsync(string cid)`
- Use a .NET IPFS client (e.g., [Ipfs.Http.Client](https://github.com/richardschneider/net-ipfs-http-client)).
- Support configuration for local node, public gateway, and pinning services.

#### b. Data Model Integration

- Return a `StorageDescriptor` populated with:
  - `protocol = "ipfs"`
  - `integrity_proof = ni:///sha-256;<digest>`
  - `location` (region, jurisdiction, provider, CID)
  - `size_bytes`, `media_type`
- Ensure compatibility with Codex Entry builder and validation.

#### c. Hashing & Canonicalization

- Compute SHA-256 hash of file content for ni-URI.
- Use RFC 6920 format for integrity proof.
- Validate CID matches file content hash.

#### d. Error Handling & Edge Cases

- Handle network errors, pinning failures, and CID mismatches.
- Support large files and streaming uploads.
- Document fallback strategies for unavailable IPFS nodes/gateways.

---

### 4. Functional Plan & Workflow

#### a. Store File in IPFS

1. Accept file stream and metadata.
2. Add file to IPFS using client library.
3. Retrieve CID and file size.
4. Compute SHA-256 hash and ni-URI.
5. Build and return `StorageDescriptor`.

#### b. Retrieve File from IPFS

- Implement retrieval by CID for verification and audit.
- Support streaming and partial reads.

#### c. Integration with Protocol Flow

- Use adapter in end-to-end flow: file → IPFS → Codex Entry → anchor → sign → verify.
- Ensure all metadata is available for audit and certificate generation.

---

### 5. Testing & Validation

- Provide unit and integration tests for:
  - File storage and retrieval
  - CID and hash validation
  - Error handling and edge cases
- Use deterministic test fixtures and mock IPFS clients for CI.
- Document testnet/mainnet setup for contributors.

---

### 6. Best Practices

- Always canonicalize and hash file content before storing.
- Use secure, reliable IPFS nodes or pinning services.
- Log all storage operations and errors for auditability.
- Document configuration and environment setup for maintainers.
- Follow .NET async/await and dependency injection patterns.

---

### 7. Reference Specification Sections

- [Storage Adapters](../../spec/storage-adapters.md)
- [Appendix A Flows](../../spec/appendix-a-flows.md)
- [Data Model](../../spec/data-model.md)
- [Reference Implementation](../../spec/reference-implementation.md)
- [RFC 6920: ni-URI](https://datatracker.ietf.org/doc/html/rfc6920)
- [IPFS Documentation](https://docs.ipfs.tech/)

---

For questions or contributions, open an issue or submit a pull request.
