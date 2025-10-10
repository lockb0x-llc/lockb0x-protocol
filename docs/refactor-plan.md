# Implementation Status (October 2025)

- Align the Lockb0x reference implementation with the minimal specification in `/spec/v0.0.1-public-draft.md`, emphasising Stellar as the canonical anchor chain.
- Provide an end-to-end workflow for creating, signing, anchoring, certifying, and verifying Codex Entries.
- Deliver modular libraries, adapters, CLI, and API surfaces that share common services and are covered by automated tests and documentation.

# Schema Change Refactoring Checklist & Plan (October 2025)

## Comprehensive Checklist

### 1. Core Module

- [x] CodexEntry model uses `anchor_ref` (replaces `tx_hash`).
- [x] Builder, validator, and canonicalizer logic match latest schema fields.
- [x] Support for new anchor types (gdrive, solid, notary, OpenTimestamps).
- [x] Extension points for custom metadata and provenance documented.

### 2. Signing Module

- [x] Signature objects and protected headers match schema.
- [x] Multi-sig, key revocation, and error handling reference correct schema fields.
- [x] Documentation/examples updated for new schema.

### 3. Anchor Modules (Stellar, Ethereum, etc.)

- [x] Anchor metadata uses `anchor_ref` and supports all anchor types.
- [x] Transaction submission and verification logic updated for new protocols.
- [x] All flows (mock, real network) return schema-compliant anchor objects.

### 4. Certificates Module

- [x] CertificateDescriptor and related models match schema.
- [x] Issuance, validation, and revocation flows use correct fields.
- [x] Support for new certificate formats expanded as needed.

### 5. Storage Module

- [x] All adapters (IPFS, S3, GDrive, Solid, etc.) return required metadata per schema.
- [x] Integrity proof logic uses ni-URI consistently.
- [x] Location, provider, and jurisdiction fields updated.

### 6. Verifier Module

- [x] Verification pipeline uses new schema fields (anchor_ref, new anchor types).
- [x] All validation steps and error reporting match updated schema.

### 7. API & CLI

- [ ] Endpoint payloads and CLI commands match schema.
- [ ] OpenAPI documentation and CLI usage examples expanded.

### 8. Tests

- [x] Test vectors and fixtures use new schema fields.
- [x] Tests for new anchor types and protocols added.
- [x] Multi-sig, revision chain, and certificate flows validated.

### 9. Documentation

- [ ] AGENTS.md, README.md, and workflow docs reference new schema.
- [ ] Example flows for all anchor types and protocols expanded.
- [ ] Contributor setup and integration steps documented for new protocols.

## Actionable Refactoring Steps

1. **Final Inventory**: Search for any remaining references to deprecated fields (e.g., `tx_hash`) in code, tests, and docs.
2. **API/CLI Review**: Update endpoint payloads, CLI commands, and OpenAPI docs to match the latest schema.
3. **Documentation Sync**: Expand AGENTS.md, README.md, and workflow documentation for new anchor types and protocols.
4. **Contributor Guide**: Add step-by-step setup and integration instructions for new adapters and anchor types.
5. **Validation**: Run all tests and CI to confirm compliance and interoperability.
6. **Review**: Perform a final review of all modules and documentation for schema alignment.

---

**Status:** Most refactoring tasks for schema changes are complete. Remaining work is focused on API/CLI payloads, documentation, and contributor guides for new anchor types and protocols.

## 2. Current State Review

- Projects are mostly scaffolds (`Lockb0x.Core`, `Lockb0x.Storage`, etc.) with placeholder classes and limited functionality.
- No canonical Codex Entry model, canonicalisation, or schema validation is implemented.
- Storage adapters, signing services, Stellar anchoring, verifier pipeline, and certificate emission are missing or stubbed.
- CLI/API only provide informational commands without executing the reference workflow.

## 3. Target Architecture

- **Core library**: Codex Entry models, JSON schema loading, RFC 8785 canonicalisation helpers, revision chain utilities, provenance metadata builders, and ni-URI helpers.
- **Storage adapters**: `IStorageAdapter` interface with implementations for IPFS, S3-compatible (AWS S3/MinIO), Google Cloud Storage, and a local/mock adapter for tests. Each adapter returns integrity proofs, media metadata, and jurisdiction info per `/spec/storage-adapters.md`.
- **Signing subsystem**: JOSE/COSE services supporting Ed25519, secp256k1 (ES256K), and RSA (RS256) algorithms. Incorporate multi-signature policy evaluation and key management utilities for Stellar accounts, DIDs, and CAIP-10 identifiers.
- **Stellar anchoring**: Service that hashes canonical Codex Entries, builds Horizon transactions with `MemoHash`, embeds CAIP-2 `stellar:<network>` identifiers, and records transaction hash, ledger, and timestamp. Include verification helpers to query Horizon and validate anchors.
- **Verifier library**: Pipeline enforcing canonicalisation, schema validation, signature verification, integrity proof verification, storage metadata checks, anchor verification (Stellar), encryption policy validation, and revision traversal.
- **Certificates**: JSON certificate emitter binding integrity proofs, signatures, and Stellar anchors. Provide hooks for VC/X.509 formats and revocation registries.
- **CLI & API**: Shared services via dependency injection. CLI commands for `init`, `ingest`, `sign`, `anchor`, `certify`, `verify`, `history`. REST API exposing create/verify/certify endpoints for automation scenarios.
- **Tests & tooling**: Deterministic fixtures, Stellar testnet integration tests, adapter mocks, and verification regression tests. Provide developer setup docs and example workflows following `/spec/appendix-a-flows.md`.

## 4. Work Breakdown Structure

1. **Foundation Setup**
   - Restructure solution into cohesive projects with shared abstractions.
   - Introduce configuration system (JSON/environment) and dependency injection.
   - Add shared logging/diagnostics utilities.
2. **Data Model & Canonicalisation**
   - Implement Codex Entry model aligned with `/spec/data-model.md` and `/spec/appendix-b-schema.md`.
   - Add JSON schema validation, canonical JSON (RFC 8785) serialization, ni-URI utilities, and provenance metadata builders.
3. **Storage Layer**
   - Define adapter interfaces and metadata contracts.
   - Implement IPFS, S3-compatible, GCS, and local/mock adapters with integrity proof generation and jurisdiction metadata capture.
   - Provide adapter configuration loaders and credential handling guidance.
4. **Signing & Key Management**
   - Implement JOSE/COSE signing services with EdDSA, ES256K, and RS256.
   - Support multi-signature policies, key stores, and key provenance validation (Stellar accounts, DIDs, CAIP-10).
5. **Stellar Anchoring**
   - Integrate Horizon client for testnet/mainnet selection.
   - Build transactions embedding Codex Entry hash (MemoHash) and CAIP-2 identifiers; expose submission, status polling, and receipt recording.
   - Implement verification helpers to re-fetch transactions, validate ledger timestamps, and confirm hash integrity.
6. **Verification Pipeline**
   - Compose verification sequence covering canonicalisation, signatures, storage checks, encryption policy (per `/spec/encryption.md`), Stellar anchor validation, and revision chains.
   - Emit structured diagnostics and machine-readable verification reports.
7. **Certificates & Revocation**
   - Generate JSON certificates binding signatures, storage proofs, and anchors.
   - Provide optional VC/X.509 wrappers and revocation hooks following `/spec/certificates.md`.
8. **CLI & API Enhancements**
   - Wire services into CLI commands covering the full workflow.
   - Implement REST endpoints with JSON I/O, authentication extension points, and streaming for large files.
   - Provide configuration templates and example workflows.
9. **Testing, Samples, and Documentation**
   - Create unit/integration tests and publish test vectors.
   - Document setup for adapters, keys, Stellar anchoring, and verification scenarios.
   - Supply sample entries and certificates for interoperability testing.

## 5. Milestones & Deliverables

- **Milestone 1**: Core data model, canonicalisation, and storage adapter interfaces complete with tests.
- **Milestone 2**: Signing subsystem and Stellar anchoring end-to-end on testnet with verification support.
- **Milestone 3**: CLI/API workflows operational; JSON certificates issued; documentation and examples published.
- **Milestone 4**: Comprehensive test suite (unit + integration) and CI configuration.

## 6. Dependencies & Risks

- Availability and stability of Stellar Horizon client and network access for integration tests.
- Cryptographic library compliance with required algorithms and safe key handling.
- Secure management of adapter credentials and prevention of sensitive data leakage.
- Ensuring canonicalisation and hashing remain deterministic across platforms.
- Managing complexity of multi-signature policies and revision chains while keeping UX approachable.

## 7. Next Steps

## 8. Immediate Next Steps for Schema Change Refactoring

- [ ] Complete API/CLI and documentation updates for new schema and anchor types.
- [ ] Add/expand contributor guides for new adapters and protocols.
- [ ] Validate all modules and tests for full schema compliance.

---

For questions or contributions, open an issue or submit a pull request.
