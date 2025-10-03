# Lockb0x.Verifier — AGENTS.md

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
