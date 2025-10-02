# Implementation Status (October 2025)

- Only **Lockb0x.Core** and **Lockb0x.Signing** modules are implemented and tested.
- Storage, anchoring, certificate, and verifier modules are not yet available; their APIs and flows are documented for future work.
- All tests and workflows currently focus on canonicalization, Codex Entry modeling, and cryptographic signing/verification (Ed25519, ES256K, RS256).
- Multi-signature policies, key revocation, and error handling are covered in the Signing module and its tests.
- See `Lockb0x.Tests/SigningServiceTests.cs` and `Lockb0x.Tests/CoreTests.cs` for reference test coverage.

# Lockb0x Technical Design — Core, Signing, and Stellar Anchoring Modules

## 1. Purpose and Scope

This document finalizes the technical design for the `Lockb0x.Core`, `Lockb0x.Signing`, and `Lockb0x.Anchor.Stellar` modules. It refines the responsibilities defined in the refactor plan by grounding them in the normative requirements of the Lockb0x specification v0.0.1. The design emphasises Stellar as the mandatory anchoring network and ensures the reference implementation demonstrates the full Codex Entry lifecycle from creation through verification.

The following specification sections inform this design:

- Codex Entry data model and required fields (§3, `/spec/data-model.md`).
- Signature formats, algorithms, and verification rules (§6, `/spec/signatures.md`).
- Anchoring workflow, Stellar constraints, and verification (§5, `/spec/anchoring.md`).
- Reference implementation goals and workflow expectations (§12, `/spec/reference-implementation.md`).

## 2. Lockb0x.Core Module

### 2.1 Responsibilities

- Provide strongly typed models that map 1:1 to the Codex Entry schema, enforcing required fields (`id`, `version`, `storage`, `identity`, `timestamp`, `anchor`, `signatures`) and conditional rules for revisions and encryption metadata as defined in §3.2.【F:spec/data-model.md†L31-L86】
- Supply canonical JSON serialisation utilities implementing RFC 8785 to guarantee deterministic payloads before signing and verification per §6.3.【F:spec/signatures.md†L27-L36】
- Enforce integrity proof semantics by normalising provider identifiers (IPFS CIDs, S3 ETags, etc.) into RFC 6920 ni-URIs before inclusion in entries, aligning with §3.2 and §12.2.【F:spec/data-model.md†L43-L55】【F:spec/reference-implementation.md†L31-L44】
- Manage provenance linkage via `previous_id` revision chains and optional `wasDerivedFrom` metadata, ensuring verifiers can traverse histories (§3.5, §12.2).【F:spec/data-model.md†L111-L116】【F:spec/reference-implementation.md†L49-L56】
- Provide validation pipelines combining JSON schema checks (Appendix B), semantic validation (identity hierarchy), and adapter metadata sanity checks.

### 2.2 Data Flow Overview

1. **Ingest asset metadata** from storage adapters, yielding ni-URI proofs, MIME type, size, and location.
2. **Compose Codex Entry draft** with identity, timestamp, and optional encryption metadata.
3. **Canonicalise and hash** the entry payload, returning digests to the Stellar anchoring service and signing module.
4. **Persist revision metadata** by linking `previous_id` and provenance statements.
5. **Emit validation results** for downstream signing/anchoring and expose structured errors.

### 2.3 API Contracts

- `CodexEntry` class with immutable properties and builder pattern enforcing required fields.
- `ICodexEntryValidator` interface returning structured validation results (success flag, error codes, contextual metadata).
- `IJsonCanonicalizer` service providing `string Canonicalize(object payload)` and `byte[] Hash(object payload, HashAlgorithm alg)` helpers.
- `IRevisionGraph` abstraction to walk `previous_id` chains and surface divergence or missing links.

### 2.4 Error Handling Policy

- Validation errors must return machine-readable codes (e.g., `core.validation.missing_field`, `core.identity.invalid_hierarchy`) and include offending field paths.
- Hashing or canonicalisation failures raise retriable exceptions tagged with correlation identifiers to facilitate CLI/API logging.
- Revision traversal surfaces non-fatal warnings when encountering incomplete chains but MUST escalate to errors when a required predecessor is missing for verification.

### 2.5 Extensibility Considerations

- Support extension metadata via typed `ExtensionPoint` interfaces that can project JSON-LD contexts into the `extensions` object without breaking canonicalisation.
- Allow pluggable schema validators so future protocol versions or custom profiles can apply additional rules.
- Provide adapter registries for custom storage backends as long as they emit ni-URIs and jurisdiction metadata consistent with §3.2.【F:spec/data-model.md†L43-L69】

## 3. Lockb0x.Signing Module

### 3.1 Responsibilities

- Canonicalise Codex Entries received from `Lockb0x.Core` and orchestrate signature creation after the anchor is embedded, upholding §6.1 and §6.3 requirements.【F:spec/signatures.md†L9-L36】
- Implement JOSE JWS and COSE Sign1 signing paths with required algorithms (EdDSA, ES256K, RS256) and optional header support per §6.5.【F:spec/signatures.md†L45-L60】
- Manage multi-signature thresholds driven by `encryption.policy` and ensure `last_controlled_by` reflects the actual signing keys (§6.2).【F:spec/signatures.md†L16-L26】
- Expose verification routines that validate canonical payloads, signer identities, and anchor inclusion, rejecting entries when any rule fails (§6.4).【F:spec/signatures.md†L37-L44】

### 3.2 Data Flow Overview

1. Receive canonical JSON payload and anchor metadata from `Lockb0x.Core`.
2. Resolve signer keys via DID, Stellar account, or CAIP-10 identifiers; hydrate JOSE/COSE headers with `kid` references.
3. Produce detached or embedded signatures, updating the Codex Entry `signatures` array.
4. On verification, re-canonicalise payloads, evaluate signature validity, check policy thresholds, and propagate diagnostics back to verifiers.

### 3.3 API Contracts

- `ISigningService` interface exposing:
  - `SignatureResult Sign(CodexEntry entry, SigningRequest request)`
  - `VerificationResult Verify(CodexEntry entry, VerificationPolicy policy)`
- `IKeyResolver` abstraction for mapping `kid` values to public keys (Stellar Horizon, DID resolution, key stores).
- `IMultiSigPolicyEvaluator` returning threshold decisions and tracking `last_controlled_by` obligations.

### 3.4 Error Handling Policy

- Signature creation failures (missing keys, unsupported algorithms) raise typed exceptions (`signing.key_not_found`, `signing.algorithm_unsupported`).
- Verification returns structured results enumerating invalid signatures, mismatched anchors, or policy violations; multi-sig shortfalls MUST include required vs. present counts.
- Canonicalisation mismatches result in a high-severity error instructing consumers to recompute the Codex Entry via `Lockb0x.Core` utilities.

### 3.5 Extensibility Considerations

- Plug-in providers can register additional JOSE/COSE algorithms if they map to RFC-compliant suites and pass canonicalisation tests.
- Key resolvers can be extended for hardware security modules or custody services without altering signing APIs.
- Policy evaluators should support future governance models (e.g., weighted voting) while continuing to emit deterministic threshold decisions.

## 4. Lockb0x.Anchor.Stellar Module

### 4.1 Responsibilities

- Derive Codex Entry hashes from canonical payloads and construct Stellar transactions that fit memo constraints while maintaining traceability (§5.1–§5.2.1).【F:spec/anchoring.md†L9-L47】
- Encode memo identifiers using the MD5+public key strategy recommended for Stellar and persist canonical SHA-256 digests in the `anchor` object (`hash_alg`) for verifier use (§5.2.1).【F:spec/anchoring.md†L25-L40】
- Submit transactions to Horizon, capture `tx_hash`, ledger sequence, and timestamp, and surface them to `Lockb0x.Core` for inclusion in Codex Entries (§5.3).【F:spec/anchoring.md†L49-L65】
- Provide verification utilities that recompute memo identifiers, query Horizon for transaction details, and cross-check ledger timestamps with Codex Entry expectations (§5.4).【F:spec/anchoring.md†L67-L87】

### 4.2 Data Flow Overview

1. Accept canonical hash material from `Lockb0x.Core`.
2. Generate Stellar memo identifiers (MD5 digest + signer key), craft transactions, and broadcast via Horizon (testnet or mainnet based on configuration).
3. Return anchor artifacts: `chain` (`stellar:pubnet` or `stellar:testnet`), `tx_hash`, memo identifier, ledger timestamp, and optional `token_id` for NFT-based anchors.
4. During verification, reconstruct memo identifier, fetch transaction data, validate inclusion of digest, confirm timestamp ordering, and reconcile signing accounts.

### 4.3 API Contracts

- `IStellarAnchorService` interface with:
  - `AnchorResult AnchorAsync(CodexEntry entry, Network network, Account signer)`
  - `AnchorVerificationResult VerifyAsync(CodexEntry entry, AnchorProof proof)`
- `IHorizonClient` abstraction to encapsulate Horizon REST calls (submit transaction, fetch transaction/ledger, resolve accounts).
- `IMemoStrategy` interface for deriving memo payloads, enabling alternate encoding strategies if Stellar increases memo sizes.

### 4.4 Error Handling Policy

- Network/transaction failures return retryable errors with Horizon response metadata; repeated failures should trigger circuit-breaker behaviour to avoid rate limits.
- Memo overflow or digest mismatch errors are treated as non-retryable configuration faults and bubble back to `Lockb0x.Core` for operator intervention.
- Verification exposes explicit failure reasons (`anchor.tx_not_found`, `anchor.hash_mismatch`, `anchor.timestamp_inconsistent`) that upstream verifiers convert into fatal validation results (§5.4).【F:spec/anchoring.md†L67-L87】

### 4.5 Extensibility Considerations

- Allow injection of alternative anchoring strategies (e.g., Merkle batching) via the `IMemoStrategy` and future `IAnchorPayloadBuilder` interfaces.
- Support multi-chain anchoring by abstracting network identifiers while ensuring Stellar remains the default per §5.2 and §12.2.【F:spec/anchoring.md†L19-L24】【F:spec/reference-implementation.md†L25-L38】
- Provide hooks for observability (metrics, structured logs) so operators can integrate with monitoring stacks when scaling Horizon interactions.

## 5. Cross-Module Integration

- `Lockb0x.Core` orchestrates data assembly, canonicalisation, and validation before invoking signing and anchoring services.
- Anchoring must occur before signing; the Stellar module returns anchor metadata that the signing module then covers within the canonical payload, satisfying §6 workflow ordering.【F:spec/signatures.md†L5-L13】【F:spec/reference-implementation.md†L58-L70】
- Verification flows coordinate `Lockb0x.Signing` and `Lockb0x.Anchor.Stellar` outputs, enforcing that both signature validity and anchor integrity pass before certificates are issued (§5.4, §6.4, §12.3).【F:spec/anchoring.md†L67-L87】【F:spec/signatures.md†L37-L44】【F:spec/reference-implementation.md†L58-L70】

## 6. Implementation Roadmap Alignment

- Align Milestone 1 with delivering the core module models, validators, and canonicalisation utilities.
- Prioritise Stellar anchoring integration (Milestone 2) to satisfy mandatory blockchain support before expanding adapters or certificate formats.
- Ensure test coverage includes deterministic vectors for canonicalisation, signature verification, and Stellar anchor reconciliation to uphold interoperability goals outlined in §12.4.【F:spec/reference-implementation.md†L72-L85】

By adhering to this design, the reference implementation will meet the minimal specification while providing clear extension points for future protocol revisions and additional anchoring networks.
