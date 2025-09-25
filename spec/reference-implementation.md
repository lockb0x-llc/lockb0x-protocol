


# 12. Reference Implementation (Non-Normative)

The Lockb0x Protocol includes a reference implementation to guide developers and provide a baseline for interoperability.  
This section is informative and does not define new normative requirements.

---

## 12.1 Goals of the Reference Implementation

- Demonstrate the end-to-end workflow of creating, anchoring, signing, and verifying Codex Entries.  
- Provide working adapters for common storage systems (IPFS, S3, Google Cloud Storage, Azure Blob).
- Showcase blockchain anchoring with Stellar as the primary example.  
- Offer CLI and API tools to simplify integration.  
- Act as a foundation for conformance testing.  
- Support both encrypted and plaintext assets throughout the workflow.  
- Enforce the required identity hierarchy: `org`, `process`, and `artifact` are required, while `subject` is optional.  

---

## 12.2 Components

The reference implementation SHOULD include:

- **CLI Tool** (`lockb0x-cli`)  
  - Commands to generate Codex Entries.  
  - Functions to anchor, sign, and verify entries (reflecting the workflow order: anchor before signing).  
  - Options to export certificates (JSON, VC, X.509).  

- **API Service** (`lockb0x-api`)  
  - RESTful endpoints for creating, anchoring, signing, and verifying Codex Entries.  
  - Integration with external storage adapters.  
  - Middleware for certificate issuance.  

- **Storage Adapters**
  - Reference adapters for IPFS, S3, Google Cloud Storage, and Azure Blob.
  - Mock adapters for testing (local filesystem, memory).  
  - Adapters MUST output ni-URI proofs for asset references. Provider-specific identifiers (CIDs, ETags, etc.) MUST be canonically mapped to ni-URIs before inclusion in Codex Entries.  

- **Verifier Library**  
  - Functions to validate signatures, storage proofs, anchors, and certificates.  
  - Support for revision chain traversal.  
  - Responsible for validating the `last_controlled_by` field and handling multi-signature scenarios.  
  - Must handle both plaintext and encrypted Codex Entries.  
  - Enforces identity hierarchy rules: `org`, `process`, and `artifact` are required, `subject` is optional.  
  - Traverses revision chains using both `previous_id` and `wasDerivedFrom` fields.  

- **Certificate Generator**  
  - Exports certificates in JSON, VC, and X.509 bindings.  
  - Provides utilities for revocation and status endpoints.  

---

## 12.3 Example Workflow

1. User runs `lockb0x-cli create` to generate a Codex Entry for a file.  
2. Entry is anchored to Stellar (`lockb0x-cli anchor`).  
3. Entry is signed locally using the user’s key(s).  
4. A certificate is generated (`lockb0x-cli certify --format json`).  
5. Another user retrieves the certificate and verifies it using the verifier library.  

---

## 12.4 Testing and Interoperability

- The reference implementation SHOULD include automated test suites for verifying compliance with this specification.  
- Implementations MAY use the reference implementation as a conformance baseline.  
- Test vectors (sample Codex Entries, signatures, anchors) MUST be provided, and MUST cover both plaintext and encrypted assets.  
- Test vectors MUST include multi-signature scenarios using the `last_controlled_by` field.  
- Test vectors MUST exercise revision chain traversal using both `previous_id` and `wasDerivedFrom`.  

---

## 12.5 Availability

The reference implementation SHOULD be published under an open-source license to encourage adoption and contributions.  
A GitHub repository MAY host the code, documentation, and issue tracking for community feedback.  

---