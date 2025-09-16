

# Lockb0x Protocol

The **Lockb0x Protocol** is an open standard for proving the **existence, integrity, and custodianship** of digital data.  
It’s designed for developers who need **verifiable audit trails** without locking into a single vendor, storage backend, or blockchain.

At its core, Lockb0x provides a portable, signed JSON structure called a **Codex Entry**.  
Each entry links together:

- **Storage Proofs** — showing *where* data is stored (IPFS, S3, Azure Blob, FTP/SFTP, or local storage).  
- **Integrity Proofs** — hashes and checksums that prove the data hasn’t been tampered with.  
- **Signatures** — cryptographic attestations from the data owner or organization.  
- **Anchors** — blockchain transactions (e.g. Stellar, Ethereum, Avalanche) that provide immutable timestamps.  

By combining these, developers can create **tamper-evident, verifiable chains of custody** for files, records, or datasets.

---

## Why Developers Use Lockb0x

- **Cross-Backend Portability**: works the same whether you use IPFS, S3, or a private server.  
- **Standards-Aligned**: builds on RFC 6920 (integrity URIs), RFC 7515 (JOSE signatures), W3C DIDs, and Verifiable Credentials.  
- **Easy Verification**: a simple verifier can check hashes, signatures, and anchors.  
- **Interoperable**: doesn’t replace your storage or blockchain — it ties them together.  
- **Compliance Ready**: supports legal frameworks like GDPR (EU) and UCC Section 12 (US).  

---

## Example Use Cases

- Proving a dataset hasn’t changed between research collaborators.  
- Providing auditors with cryptographically verifiable compliance documents.  
- Anchoring invoices, contracts, or deliverables for cross-organization projects.  
- Creating a digital chain of custody for supply chain or legal evidence.  

---

## Specification

The full technical specification lives in [`spec/v1.0.0-public-draft.md`](spec/v1.0.0-public-draft.md).  
Each section of the spec is broken out into its own file in the `spec/` folder for clarity.  

---

## Contributing

Lockb0x is at an early stage and we welcome feedback, contributions, and discussion.  
- Open issues to suggest improvements or report problems.  
- Submit pull requests to add adapters, verifiers, or clarifications.  
- Join the discussion on standards alignment and compliance use cases.  

---

## License

This project is licensed under the [MIT License](LICENSE).
# Lockb0x Protocol

**Lockb0x Protocol** is an open **standards-track specification** for proving the **existence, integrity, provenance, and custodianship** of digital data.
It defines a portable, signed JSON structure called a **Codex Entry** and the rules for storing, anchoring, certifying, and verifying those entries across diverse systems.

Unlike a storage system or blockchain, Lockb0x is a **verification and provenance layer** that ties them together.
It is designed for **interoperability, auditability, and compliance** across decentralized and traditional environments.

---

## What It Is

- A **protocol specification**
  - Defines the data model, storage adapters, anchoring methods, signatures, identity rules, verification processes, certificates, and security requirements.
  - Uses normative requirements (MUST/SHOULD/MAY) for consistent, interoperable implementations.
  - Aligned with established standards (RFC 6920, RFC 7515, CAIP-2, W3C DIDs, W3C Verifiable Credentials, W3C PROV-DM).

- A **reference implementation**
  - Includes a CLI and API service to demonstrate end-to-end workflows.
  - Provides adapters for IPFS, S3, Azure Blob, FTP/SFTP, and local storage.
  - Anchors entries to Stellar (required) with optional support for other blockchains (Ethereum, Avalanche, etc.).
  - Ships with verifiers, certificate generators, and test vectors for conformance.

---

## Why It Matters

- **Data Sovereignty**: empowers individuals and organizations to retain control over their data across jurisdictions.
- **Compliance**: supports frameworks such as GDPR (EU), eIDAS, and UCC Section 12 (US) by cryptographically binding provenance and jurisdictional metadata.
- **Interoperability**: bridges decentralized storage, enterprise infrastructure, and blockchain anchors.
- **Auditability**: provides tamper-proof audit trails across revisions, multi-sig contexts, and multi-party workflows.

---

## How It Works

1. **Codex Entry**: a signed JSON record that includes storage metadata, integrity proofs, signatures, identity, and anchors.
2. **Storage Adapters**: standardized connectors produce verifiable integrity proofs across backends.
3. **Anchoring**: blockchain transactions serve as immutable timestamps and jurisdictional attestations.
4. **Certificates**: machine- and human-readable attestations (JSON, W3C Verifiable Credential, X.509).
5. **Verification**: tools validate integrity, signatures, anchors, and revision chains.

---

## Example Use Cases

- Research: prove datasets remain unchanged between collaborators.
- Compliance: provide auditors with cryptographically verifiable reports.
- Finance & Legal: anchor invoices, contracts, or work orders in cross-org workflows.
- Supply Chain: ensure digital custody and authenticity of records or evidence.
- Cloud/Hybrid IT: maintain sovereignty across multiple providers and jurisdictions.

---

## Repository Structure

- `/spec/` — The normative specification (Markdown).
- `/cli/` — Reference CLI implementation (`lockb0x-cli`).
- `/api/` — Reference API service (`lockb0x-api`).
- `/tests/` — Test vectors and conformance tools.

---

## Getting Started

- Read the [Specification](./spec/v1.0.0-public-draft.md) for the full draft protocol definition.
- Try the [Reference Implementation](./cli/) to generate and verify Codex Entries.
- Explore [Example Flows](./spec/appendix-a-flows.md) and [JSON Schema](./spec/appendix-b-schema.md).

---

## Status

- Current Draft: **v1.0.0 (Candidate)**
- Goal: Submit for review to standards bodies (W3C CCG, IETF, ISO).
- Contributions: Feedback, adapters, test cases, and verifiers are welcome.

---

## License

The Lockb0x Protocol specification and reference implementation are released under the **Apache 2.0 License**.
