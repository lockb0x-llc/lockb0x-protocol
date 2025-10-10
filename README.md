![Lockb0x Protocol](signed-sealed-delivered.png)

# Lockb0x Protocol

At its core, Lockb0x provides a portable, signed JSON structure called a **Codex Entry**.

Each entry links together:

- **Storage Proofs** — showing _where_ data is stored (IPFS, S3, Azure Blob, Google Drive, FTP/SFTP, or local storage).
- **Integrity Proofs** — hashes and checksums (RFC 6920 ni-URIs, SHA-256, MD5, etc.) that prove the data hasn’t been tampered with.
- **Signatures** — cryptographic signatures/attestations from the data owner or organization (JOSE, X.509, W3C VC, etc.).
- **Anchors** — immutable timestamping and provenance records, which may be blockchain transactions (Stellar, Ethereum, Avalanche), distributed ledger entries, or standards-based notary/attestation services (RFC 3161, OpenTimestamps, Google Drive revision history, etc.).

By combining these, developers can create **tamper-evident, verifiable chains of custody** for files, records, or datasets, regardless of storage backend or anchoring technology.

---

**Protocol Development Status (October 2025):**

The Lockb0x Protocol reference implementation now includes:

- **Core**: Data model, canonicalization, and validation (fully implemented and tested)
- **Signing**: Cryptographic signing and verification (Ed25519, ES256K, RS256; fully implemented and tested)
- **Storage**: IPFS adapter implemented and tested; S3, Filecoin, Azure Blob, and new Google Drive adapter in progress
- **Anchor.Stellar**: Mock/in-memory anchoring implemented and tested; real Stellar network integration pending
- **Anchor.Eth**: Ethereum smart contract and SDK implemented; testnet deployment and verification workflows available
- **Anchor.GDrive**: Reference implementation for Google Drive anchoring and revision history as a non-blockchain anchor
- **Verifier**: Fully implemented and tested. The pipeline orchestration, stepwise verification logic, and all verification steps (schema, canonicalization, integrity, signatures, anchor, revision chain, certificate) are covered by deterministic unit tests. IPFS + Stellar and Ethereum verification flows are implemented and passing.
- **Certificates**: Interfaces and models present; certificate emission and revocation logic planned
- **CLI & API**: CLI exists but is not yet integrated for end-to-end flows; API is planned
- **Tests**: All modules covered by unit tests; all tests pass except Secp256k1 (platform limitation)

**Schema Changes and Improvements:**

- The Codex Entry schema now supports:
  - Multiple anchor types (blockchain, notary, DLT, GDrive, RFC 3161, OpenTimestamps)
  - Extended metadata for anchors (transaction hash, revision ID, attestation URI, etc.)
  - Flexible integrity proof formats (ni-URI, hash, checksum)
  - Improved identity and custody fields for compliance and auditability
  - Revision chain and multi-sig support

**Gaps & Next Steps:**

- Integrate real Stellar SDK/Horizon for network anchoring and verification
- Expand documentation and add more end-to-end examples for the Verifier module and protocol pipeline
- Integrate CLI and API for end-to-end flows and workflow commands
- Expand storage and anchor adapters (S3, Filecoin, Azure Blob, Google Drive, Ethereum, Avalanche, RFC 3161, OpenTimestamps, etc.)
- Add contributor guide, CLI usage docs, and end-to-end examples
- Refactor error handling and implement centralized logging

For details, see [`docs/AGENTS.md`](docs/AGENTS.md), module-specific documentation, and the updated protocol schema in [`spec/`](spec/).

Plain English: Store any kind of data, encrypted or not, on any storage media/platform and use any blockchain. With the lockb0x-protocol and a Codex Entry, you can always prove it hasn't been tampered with, when it was created and by "whom", and who has control or "custody" of the data/asset. If the control ever changes, the protocol follows it. The lockb0x-protocol is not a storage specification.

It is a solution for data sovereignty that also provides the basis for Controllable Electronic Records, or a CER.

The lockb0x CER is in compliance with and serves as of machine-readable implementation of a CER as defined under United States Uniform Commercial Code Sections 12, 9, 8, and 1.

Which was revised in 2022 to address and regulate using blockchain accounts and cryptocurrency in commerce. In addition to defining custody by control as a proof of ownership. I.E., your keys, your tokens; or data/documents/assets as the case may be.

It has been adopted by all 50 states and territories of the U.S. and the lockb0x protocol is intended to be used as a reference implmentation of these modernized transaction processing capabilities for use in commerce.

The lockb0x-protocol, by design supports the standards and ethos of GDPR and is an enabler of personal data ownership and
control.

The lockb0x-protocol Verifier Reference Implementation is under development.

## Contributor Guidance & Example Flow

Please fork this repo and contribute by submitting a pull-request. Use the Issues tab to ask questions, discuss, or report issues.

**Example Verification Flows:**

- IPFS + Stellar: Store file in IPFS, create Codex Entry, canonicalize, anchor on Stellar, sign, generate certificate, verify all steps.
- Ethereum: Store file, create Codex Entry, canonicalize, anchor on Ethereum, sign, verify via smart contract.
- Google Drive: Store file in GDrive, create Codex Entry, canonicalize, anchor using GDrive revision history, sign, verify via GDrive API.

See [Appendix A: Example Flows](spec/appendix-a-flows.md) for more details and schema examples.

---

## Why Developers Should Adopt the lockb0x-protocol

- **Cross-Backend Portability**: works the same whether you use IPFS, S3, Google Drive, or a private server.
- **Standards-Aligned**: builds on RFC 6920 (integrity URIs), RFC 7515 (JOSE signatures), RFC 3161 (timestamping), OpenTimestamps, W3C DIDs, and Verifiable Credentials.
- **Easy Verification**: a simple verifier can check hashes, signatures, and anchors, regardless of backend or anchor type.
- **Interoperable**: doesn’t replace your storage, blockchain, or notary — it ties them together.
- **Compliance Ready**: supports legal frameworks like GDPR (EU), UCC Section 12 (US), and modern data sovereignty standards.

---

## Example Use Cases

- Proving a dataset hasn’t changed between research collaborators.
- Providing auditors with cryptographically verifiable compliance documents.
- Anchoring invoices, contracts, or deliverables for cross-organization projects.
- Creating a digital chain of custody for supply chain or legal evidence.
- Using Google Drive revision history as a non-blockchain anchor for document provenance.

---

## Specification

The full technical specification lives in [`spec/v0.0.2-public-draft.md`](spec/v0.0.2-public-draft.md).
Each section of the spec is broken out into its own file in the [`spec/`](spec/) folder for clarity. The schema now supports multiple anchor types and extended metadata for flexible, standards-based provenance.

---

## Contributing

Lockb0x is at an early stage and we welcome feedback, contributions, and discussion.

- Open issues to suggest improvements or report problems ([GitHub Issues](https://github.com/lockb0x-llc/lockb0x-protocol/issues)).
- Submit pull requests to add adapters, verifiers, or clarifications (see [`CONTRIBUTING.md`](CONTRIBUTING.md)).
- Join the discussion on standards alignment and compliance use cases.

---

## License

This project is licensed under the [MIT License](LICENSE). The Lockb0x Protocol specification and reference implementation are released under the **Apache 2.0 License**.
