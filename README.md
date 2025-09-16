

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