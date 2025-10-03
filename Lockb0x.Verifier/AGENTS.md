# Lockb0x.Verifier — AGENTS.md

# Implementation Status (October 2025)

## Current State

- **Specification Alignment:** The technical plan and interfaces are fully aligned with `/spec/verification.md`, `/spec/certificates.md`, `/spec/appendix-a-flows.md`, and all referenced standards (RFC 8785, RFC 6920, RFC 7515, RFC 5280, W3C VC).
- **Interfaces:** `IVerifierService` and supporting models are defined, matching the protocol requirements for verification, certificate validation, and revision chain traversal.
- **Integration:** Project references and test scaffolding are in place for integration with Core, Signing, Anchor, Storage, and Certificates modules.
- **Testing:** Initial unit tests exist in `Lockb0x.Tests/VerifierServiceTests.cs` covering basic verification scenarios (valid/invalid signatures, certificate delegation).
- **Extensibility:** The design supports pluggable adapters and extension points for storage, anchor, and signature verification.

## Gaps & Progress

- **Verification Pipeline:** Most verification steps (schema, canonicalization, integrity, anchor, encryption, revision chain, certificate validation) are not yet fully implemented; only basic signature and certificate delegation logic is present.
- **Diagnostics & Error Reporting:** Structured error/warning reporting is planned but not fully implemented; current tests only check for basic error codes.
- **Integration:** Full integration with CLI, API, and other protocol modules is pending; only project references and test stubs exist.
- **Documentation:** Contributor guides, usage examples, and OpenAPI/CLI documentation are not yet available.
- **Testing:** More comprehensive unit and integration tests are needed, including deterministic vectors and edge cases from `/spec/appendix-a-flows.md`.

## Progress Toward Full Implementation

- **Interfaces and models are defined and match the spec.**
- **Basic signature and certificate verification logic is present and tested.**
- **Project references and test scaffolding enable integration with all protocol modules.**
- **Comprehensive technical plan and implementation steps are documented below.**

## Next Steps

1. Implement the full verification pipeline per `/spec/verification.md` and referenced standards.
2. Add structured diagnostics and error reporting for all verification steps.
3. Expand unit and integration tests to cover all protocol flows and edge cases.
4. Integrate Verifier with CLI and API for automated and interactive workflows.
5. Document contributor setup, usage examples, and extension patterns.
6. Provide OpenAPI documentation and CLI usage guides.

## Implementation Plan (October 2025)

This document provides a comprehensive technical and practical development plan for implementing the Lockb0x.Verifier module. The goal is to deliver a robust, standards-compliant verifier for Codex Entries, certificates, and protocol flows, fully integrated with the Lockb0x reference implementation.

---

## 1. Specification & Standards Alignment

- **Reference:**
  - `/spec/verification.md` — Verification steps and requirements
  - `/spec/appendix-a-flows.md` — End-to-end protocol flows
  - `/spec/data-model.md` — Codex Entry schema
  - `/spec/certificates.md` — Certificate verification rules
  - `/spec/reference-implementation.md` — Integration and workflow
  - RFC 8785 (JCS), RFC 6920 (ni-URI), RFC 7515 (JOSE), RFC 5280 (X.509), W3C VC

---

## 2. Core Responsibilities

- Validate Codex Entries for schema, canonicalization, integrity, signatures, anchors, encryption, and revision chain.
- Verify certificates (JSON, VC, X.509) per protocol and referenced standards.
- Provide detailed diagnostics and error reporting for failed verifications.
- Support both synchronous and async agent workflows.
- Integrate with Core, Signing, Anchor, Storage, and Certificates modules.

---

## 3. Technical Design & Interfaces

### a. Main Service Interface

- `IVerifierService`
  - `Task<VerificationResult> VerifyAsync(CodexEntry entry)`
  - `Task<VerificationResult> VerifyCertificateAsync(CertificateDescriptor certificate, CodexEntry entry)`
  - `Task<RevisionChainResult> TraverseRevisionChainAsync(string entryId)`

### b. Verification Pipeline (per `/spec/verification.md`)

1. **Schema Validation** — Validate against JSON schema; reject unknown fields.
2. **Canonicalization** — RFC 8785 JCS; ensure deterministic encoding.
3. **Signature Validation** — Verify all signatures, enforce multi-sig policies.
4. **Integrity Check** — Validate ni-URI hash matches file content.
5. **Location Verification** — Confirm storage metadata and attestation.
6. **Anchor Validation** — Confirm anchor exists on chain, validate hash and Merkle/NFT proofs.
7. **Encryption & Key Ownership** — Validate encryption metadata and control.
8. **Revision Chain Traversal** — Traverse and validate all revisions for CER compliance.
9. **Certificate Verification** — Validate format, signatures, PKI chain (X.509), and referenced entry.

### c. Extensibility & Integration

- Pluggable adapters for storage, anchor, and signature verification.
- Support for external key stores and blockchain explorers.
- Integration points for CLI, API, and agent workflows.
- Structured error and warning reporting for diagnostics.

---

## 4. Implementation Steps

## Implementation Steps (In Progress)

## Practical Implementation Plan: IPFS + Stellar Verification Pipeline

This plan details the steps to implement the basic verification pipeline for Codex Entries stored in IPFS and anchored on Stellar, as described in `/spec/appendix-a-flows.md`. It covers integration with Core, Storage, Signing, Anchor, and Certificates modules.

### 1. Entry Ingestion & Metadata

- Accept a Codex Entry and (optionally) the referenced file or its hash.
- Extract required fields: `id`, `storage.protocol`, `storage.integrity_proof`, `identity`, `anchor`, `signatures`.

### 2. Schema Validation

- Use Core module's validator to check the entry against the canonical JSON schema.
- Reject entries with unknown or missing fields.

### 3. Canonicalization

- Use Core's JCS canonicalizer to produce a deterministic encoding of the entry.

### 4. Integrity Proof Validation (IPFS)

- Use Storage module's IPFS adapter to fetch the file by CID.
- Recompute the file's hash and compare to the `storage.integrity_proof` (RFC 6920 ni-URI).
- If the hash matches, proceed; otherwise, fail with a diagnostic error.

### 5. Signature Validation

- Use Signing module to verify all signatures on the canonicalized entry.
- Enforce multi-sig policies if present.

### 6. Anchor Validation (Stellar)

- Use Anchor module to check that the entry's hash is present in the referenced Stellar transaction (MemoHash).
- Confirm the transaction exists on the declared chain/network.
- Optionally, use explorer APIs for additional validation.

### 7. Certificate Validation (if present)

- If a certificate is attached, use Certificates module to validate its format, signatures, and PKI chain (for X.509).

### 8. Diagnostics & Reporting

- Collect and return detailed results for each verification step (success/failure, error codes, warnings).
- Log all verification attempts for auditability.

### 9. Integration Points

- Core: Schema validation, canonicalization
- Storage: IPFS adapter for file retrieval and hash validation
- Signing: Signature verification
- Anchor: Stellar transaction lookup and validation
- Certificates: Certificate format and PKI validation

### 10. Testing & Examples

- Implement unit and integration tests using flows from `/spec/appendix-a-flows.md` (IPFS + Stellar, valid/invalid cases).
- Document expected outcomes and error handling for each step.

---

1. **Define Interfaces & Models**
   - Implement `IVerifierService`, `VerificationResult`, and supporting models.
   - Document all public APIs and extension points.
2. **Schema & Canonicalization Validation**
   - Integrate with Core module for schema and JCS validation.
   - Add deterministic test vectors from `/spec/appendix-a-flows.md`.
3. **Signature & Multi-Sig Validation**
   - Integrate with Signing module for signature verification.
   - Enforce threshold and key policies.
4. **Integrity & Storage Proof Validation**
   - Integrate with Storage module for ni-URI and location checks.
   - Support attestation and provider verification.
5. **Anchor Validation**
   - Integrate with Anchor module for chain and transaction verification.
   - Support Merkle/NFT proofs and explorer lookups.
6. **Encryption & Key Ownership Validation**
   - Validate encryption metadata and control policies.
   - Support multi-sig and org/custodian scenarios.
7. **Revision Chain Traversal**
   - Implement traversal logic for `previous_id` and `wasDerivedFrom` fields.
   - Validate full CER history and compliance.
8. **Certificate Verification**
   - Integrate with Certificates module for format-specific checks.
   - Validate PKI chain for X.509, VC contexts, and JSON signatures.
9. **Diagnostics & Error Reporting**
   - Provide detailed error/warning objects for all failed steps.
   - Log verification attempts and outcomes for auditability.
10. **Testing & Validation**
    - Implement unit and integration tests for all verification steps.
    - Use deterministic vectors and edge cases from `/spec/appendix-a-flows.md`.
    - Document test setup and expected outcomes.

---

## 5. Best Practices

- Use async/await and dependency injection throughout.
- Log all verification operations and errors for auditability.
- Support extension and customization via interfaces and adapters.
- Document configuration, environment setup, and integration patterns.
- Follow referenced standards for cryptography, PKI, and data validation.

---

## 6. Integration Roadmap

- Integrate Verifier with CLI and API for automated and interactive workflows.
- Provide OpenAPI documentation and CLI usage examples.
- Ensure compatibility with all protocol modules and extension points.
- Add contributor guides and end-to-end examples for practical usage.

---

## References

- [Verification Spec](../../spec/verification.md)
- [Certificates Spec](../../spec/certificates.md)
- [Appendix A: Example Flows](../../spec/appendix-a-flows.md)
- [Data Model](../../spec/data-model.md)
- [Reference Implementation](../../spec/reference-implementation.md)
- [Lockb0x Protocol Specification](../../spec/v0.0.1-public-draft.md)
- [RFC 8785: JCS](https://datatracker.ietf.org/doc/html/rfc8785)
- [RFC 6920: ni-URI](https://datatracker.ietf.org/doc/html/rfc6920)
- [RFC 7515: JOSE](https://datatracker.ietf.org/doc/html/rfc7515)
- [RFC 5280: X.509](https://datatracker.ietf.org/doc/html/rfc5280)
- [W3C Verifiable Credentials](https://www.w3.org/TR/vc-data-model/)

---

For questions or contributions, open an issue or submit a pull request.
