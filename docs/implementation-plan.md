# Implementation Status (October 2025)

- Only **Lockb0x.Core** and **Lockb0x.Signing** modules are implemented and tested.
- Storage, anchoring, certificate, and verifier modules are not yet available; their APIs and flows are documented for future work.
- All tests and workflows currently focus on canonicalization, Codex Entry modeling, and cryptographic signing/verification (Ed25519, ES256K, RS256).
- Multi-signature policies, key revocation, and error handling are covered in the Signing module and its tests.
- See `Lockb0x.Tests/SigningServiceTests.cs` and `Lockb0x.Tests/CoreTests.cs` for reference test coverage.

1. Current State Assessment

## Current State (as of October 2025)

### Lockb0x.Core

- Project targets .NET 8.0, with nullable and implicit usings enabled.
- Core folder structure includes:
  - Canonicalization (RFC 8785/JCS): interfaces and partial implementation.
  - Models: CodexEntry model started, needs expansion for all required fields.
  - Revision: revision graph and traversal logic present.
  - Utilities: ni-URI helpers present.
  - Validation: validator interfaces and partial logic present.
- No storage adapters, signing, anchoring, or certificate logic yet.

The specification expects the reference implementation to demonstrate the full workflow—create, sign, anchor (on Stellar), certify, and verify Codex Entries—with supporting adapters, verifier library, and certificate tooling.

2. Minimal Specification Surface to Cover
   Codex Entry data model: JSON structure with required fields for storage, identity, timestamp, anchor, and signatures, including revision support.

Identity rules: identifiers must be Stellar accounts, DIDs, or CAIP-10 accounts, following the canonical provenance hierarchy.

Storage adapters: at least IPFS, S3-compatible, and Google Cloud Storage adapters that emit RFC 6920 ni-URIs, jurisdiction metadata, and file size/type.

Anchoring: Stellar anchoring is mandatory; anchors must hash the Codex Entry and include CAIP-2 chain IDs, transaction hashes, and hash algorithms.

Signatures: JOSE JWS or COSE Sign1 with EdDSA, ES256K, and RS256 support; canonicalize JSON with RFC 8785 before signing/verification and honour multi-sig policies.

Verification: canonicalize, validate signatures, re-check integrity proofs, confirm location metadata, validate Stellar anchors, enforce encryption policy, and walk revision chains.

Certificates: generate at least JSON certificates binding integrity proofs, anchors, and signatures; support VC/X.509 bindings as optional extensions and provide revocation hooks.

Provenance metadata & security: preserve provenance assertions, support revision linkage, and respect security guidance on hashing, key ownership, replay protection, and jurisdiction claims.

Reference flow: exemplar sequence is file → storage adapter → signed Codex Entry → Stellar anchor → certificate → verification, which should drive CLI/API UX and testing.

3. Proposed Architectural Refactor
   Solution layout

Lockb0x.Core: Codex Entry models, JSON schema validation, canonicalization, provenance utilities, revision-chain handling.

Lockb0x.Storage: IStorageAdapter interface plus IPFS, S3-compatible, GCS, and mock adapters implementing ni-URI generation, metadata extraction, and optional notarization.

Lockb0x.Signing: JOSE/COSE services, key management, Stellar account helpers, policy evaluation for multi-sig.

Lockb0x.Anchor.Stellar: Horizon client, transaction builder embedding Codex Entry hashes (MemoHash), CAIP-2 identifiers (stellar:pubnet / stellar:testnet), verification logic.

Lockb0x.Verifier: Implements the end-to-end verification pipeline, invoking canonicalization, adapter checks, anchor verification, and revision traversal.

Lockb0x.Certificates: JSON certificate emitter (minimum), plus VC/X.509 abstractions and revocation/status utilities.

Lockb0x.Cli: Rich CLI leveraging the above services for create/sign/anchor/verify/certify flows, mirroring the spec’s sample workflow.

Lockb0x.Api: Minimal REST service offering create/verify/certify endpoints backed by the same services.

Lockb0x.Tests: Unit tests, adapter fakes, Stellar testnet integration tests, and published test vectors.

Cross-cutting concerns

Configuration system for network selection, storage credentials, key stores, and policy thresholds.

Logging/auditing aligned with provenance assertions (e.g., wasGeneratedBy, wasAnchoredIn).

Security posture (client-side encryption hooks, revocation checks, replay protection).

4. Implementation Roadmap

## Next Tasks (Milestone: Basic Reference Implementation)

### 1. Complete Core Data Model

- Expand `CodexEntry` to include all required fields per spec and example flows (id, storage, integrity_proof, identity, etc.).
- Ensure support for revision chains and provenance assertions.

### 2. Canonicalization & ni-URI

- Implement full RFC 8785 canonicalization for JSON objects.
- Finalize ni-URI helpers for all supported hash algorithms.

### 3. Validation Logic

- Expand `CodexEntryValidator` to cover all required checks:
  - Field presence and format
  - Integrity proof validation
  - Revision/provenance chain validation

### 4. Storage Adapter Interfaces

- Define interfaces for storage adapters (IPFS, S3, GCS, Solid, etc.).
- Prepare for backend-specific integrity proof mapping (see appendix-a-flows.md).

### 5. Basic Test Coverage

- Add unit tests for canonicalization, ni-URI, and validation logic.

### 6. Documentation & Example Flows

- Document the file → Codex Entry → anchor → verify flow.
- Ensure flows from appendix-a-flows.md are covered in tests and docs.

---

**Note:** Once these tasks are complete, proceed to implement storage adapters, signing subsystem, anchoring, and certificate generation as described in the technical design and refactor plan.

5. Risk & Dependency Notes
   Stellar SDK integration: confirm availability of .NET Horizon client and handle rate limiting; support memo hash length limits to fit Codex Entry hash.

Cryptography libraries: ensure JOSE/COSE packages support required algorithms without violating security policies.

Adapter credentials: abstract credential management to keep secrets out of Codex Entries; consider environment variable or secret store integration.

Schema evolution: design serialization and canonicalization to be forward-compatible (extensions, optional fields) per provenance and security guidance.

Executing this plan will transform the stub CLI into the full reference implementation mandated by the specification while emphasizing Stellar anchoring and providing the tooling, adapters, and verification workflows necessary for interoperability.
