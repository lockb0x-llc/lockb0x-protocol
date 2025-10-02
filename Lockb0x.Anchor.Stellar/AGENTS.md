# Lockb0x.Anchor.Stellar — AGENTS.md

## Implementation, Integration, Verification, and Testing Guide

---

### 1. Overview

This document provides comprehensive instructions for implementing, integrating, verifying, and testing the Stellar anchoring module in the Lockb0x Protocol reference implementation. It is grounded in the protocol specification and best practices for blockchain anchoring, cryptographic integrity, and auditability.

---

### 2. Implementation Instructions

#### a. Core Responsibilities

- Accept a canonicalized Codex Entry (RFC 8785 JSON).
- Hash the entry (SHA-256 recommended).
- Submit a Stellar transaction with the hash in the memo field (MemoHash).
- Return anchor metadata: chain/network, transaction hash, hash algorithm, timestamp.
- Provide verification helpers to confirm anchor existence and integrity on Stellar.

#### b. Interface & Service Design

- Implement `IStellarAnchorService` with methods:
  - `AnchorAsync(CodexEntry entry): Task<AnchorProof>`
  - `VerifyAnchorAsync(AnchorProof proof): Task<bool>`
- Use the Stellar SDK or Horizon API for transaction submission and lookup.
- Support configuration for testnet/mainnet, account credentials, and Horizon endpoints.

#### c. Error Handling & Edge Cases

- Handle transaction failures, network errors, and memo size limits.
- Validate that the hash algorithm and chain/network are recorded in AnchorProof.
- Ensure idempotency: avoid duplicate anchors for the same entry.

---

### 3. Integration Instructions

#### a. Workflow Integration

- Integrate with the Core module to receive canonicalized entries.
- Integrate with the Signing module to sign entries before/after anchoring as required.
- Update CodexEntry builder to include anchor metadata after successful anchoring.

#### b. Configuration

- Provide environment variables or config files for Stellar account keys, network selection, and Horizon endpoints.
- Document setup for testnet and mainnet environments.

---

### 4. Verification Instructions

#### a. Anchor Verification

- Implement `VerifyAnchorAsync` to:
  - Retrieve the transaction from Stellar using the transaction hash.
  - Confirm the memo field matches the expected hash.
  - Validate the chain/network and timestamp.
- Provide utilities to recompute the entry hash and compare with the anchored value.

#### b. Audit Trail

- Document how to traverse and validate anchor history for revision chains.
- Ensure all anchor metadata is included in certificates and audit reports.

---

### 5. Testing Instructions

#### a. Unit & Integration Tests

- Mock Stellar SDK/Horizon for unit tests.
- Provide integration tests using Stellar testnet:
  - Anchor a test entry and verify transaction on-chain.
  - Test error handling for failed transactions and invalid proofs.
- Use deterministic test fixtures for Codex Entries and anchor proofs.

#### b. Test Coverage

- Cover all edge cases: memo size, duplicate anchors, network errors, invalid hashes.
- Validate anchor verification logic against real and mocked transactions.

---

### 6. Best Practices

- Always canonicalize entries before hashing and anchoring.
- Use secure key management for Stellar accounts.
- Prefer testnet for development and CI; mainnet for production.
- Log all anchor operations and errors for auditability.
- Document all configuration and integration steps for future maintainers.

---

### 7. Reference Specification Sections

- [Anchoring](../../spec/anchoring.md)
- [Data Model](../../spec/data-model.md)
- [Appendix A Flows](../../spec/appendix-a-flows.md)
- [Reference Implementation](../../spec/reference-implementation.md)

---

For questions or contributions, open an issue or submit a pull request.
