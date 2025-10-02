# Implementation Status (October 2025)

- **Lockb0x.Core** and **Lockb0x.Signing** are the only modules currently implemented and tested.
- Storage, anchoring, certificate, and verifier modules are not yet implemented; their APIs and flows are documented for future work.
- All tests and workflows currently focus on canonicalization, Codex Entry modeling, and cryptographic signing/verification (Ed25519, ES256K, RS256).
- Multi-signature policies, key revocation, and error handling are covered in the Signing module and its tests.
- See `Lockb0x.Tests/SigningServiceTests.cs` and `Lockb0x.Tests/CoreTests.cs` for reference test coverage.

# AGENTS.md

## Lockb0x Protocol — Technical Design for AI Agents

This document provides detailed technical design specifications for the Lockb0x Protocol core, signing, and anchoring modules, enabling interoperability and extension by other AI Agents. It is based on the [Lockb0x Protocol Specification v0.0.1](../spec/v0.0.1-public-draft.md).

---

## 1. Core Module

### Responsibilities

- Define the Codex Entry data model (JSON structure).
- Provide canonicalization (RFC 8785 JCS).
- Validate entries against the protocol schema.
- Manage provenance and revision chains.

### Data Flows

- Ingest file metadata via storage adapters.
- Construct Codex Entry objects with required and optional fields.
- Canonicalize entries before signing and anchoring.
- Traverse revision chains for audit and verification.

### API Contracts

- `CreateEntry(metadata: object): CodexEntry`
- `Canonicalize(entry: CodexEntry): string`
- `Validate(entry: CodexEntry): ValidationResult`
- `GetRevisionChain(entryId: string): CodexEntry[]`

### Error Handling

- Return structured error objects with codes and messages.
- Validation errors include field-level diagnostics.
- Canonicalization failures raise explicit exceptions.

### Extensibility

- Support for custom metadata extensions (JSON-LD).
- Pluggable schema validation.
- Adapter pattern for provenance and revision traversal.

---

## 2. Signing Module

### Responsibilities

- Implement JOSE JWS and COSE Sign1 signing and verification.
- Support EdDSA (Ed25519), ES256K (secp256k1), and RS256 algorithms.
- Enforce multi-sig policies and key revocation checks.

### Data Flows

- Accept canonicalized Codex Entry payloads.
- Generate signature objects with algorithm, key ID, and signature value.
- Attach signatures to Codex Entries.
- Verify signatures during entry validation.

### API Contracts

- `Sign(payload: byte[], key: SigningKey, alg: string): SignatureProof`
- `Verify(payload: byte[], signature: SignatureProof): bool`
- `ListKeys(): SigningKey[]`
- `RevokeKey(keyId: string): void`

### Error Handling

- Signature failures return algorithm/key-specific error codes.
- Revoked or expired keys trigger explicit validation errors.
- Multi-sig policy violations are surfaced as threshold errors.

### Extensibility

- Add support for new algorithms via strategy pattern.
- Integrate external key stores (HSM, DID, Stellar).
- Custom signature formats via interface extension.

---

## 3. Anchoring Module

### Responsibilities

- Anchor Codex Entries on blockchains (Stellar required, others optional).
- Generate anchor objects with chain ID, transaction hash, hash algorithm, and optional token ID.
- Validate anchor existence and payload inclusion.

### Data Flows

- Accept signed Codex Entries for anchoring.
- Submit transactions to blockchain networks (e.g., Stellar MemoHash).
- Retrieve and validate anchor metadata during verification.

### API Contracts

- `Anchor(entry: CodexEntry, network: string): AnchorProof`
- `VerifyAnchor(anchor: AnchorProof, entry: CodexEntry, network: string): bool`
- `GetTransactionUrl(anchor: AnchorProof, network: string): string`

### Error Handling

- Blockchain submission errors include network and transaction diagnostics.
- Anchor verification failures return missing/invalid transaction errors.
- Timestamp and hash mismatches are reported with context.

### Extensibility

- Support additional blockchains via adapter interface.
- Custom anchor payload formats.
- Pluggable verification logic for new ledger types.

---

## General Considerations

- All modules expose async APIs for integration with agent workflows.
- Error objects are machine-readable for automated diagnostics.
- Extension points are documented for agent interoperability.
- All data models align with the canonical schema in [`spec/v0.0.1-public-draft.md`](../spec/v0.0.1-public-draft.md).

---

For further details, see [Appendix B: JSON Schema](../spec/appendix-b-schema.md) and [Appendix A: Example Flows](../spec/appendix-a-flows.md).
