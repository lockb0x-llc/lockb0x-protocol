

# Lockb0x Protocol

The **Lockb0x Protocol** is an open standard for proving the **existence, integrity, and custodianship** of digital data.  
It’s designed for developers who need **verifiable audit trails** without locking into a single vendor, storage backend, or blockchain.

At its core, Lockb0x provides a portable, signed JSON structure called a **Codex Entry**.  
Each entry links together:

- **Storage Proofs** — showing *where* data is stored (IPFS, S3, Azure Blob, FTP/SFTP, or local storage).  
- **Integrity Proofs** — hashes and checksums that prove the data hasn’t been tampered with.  
- **Signatures** — cryptographic signatures/attestations from the data owner or organization.  
- **Anchors** — blockchain transactions (e.g. Stellar, Ethereum, Avalanche) that provide immutable timestamps.  

By combining these, developers can create **tamper-evident, verifiable chains of custody** for files, records, or datasets.

---
# Plain English: Store any kind of data, encrypted or not on any storage media/platform, and you can always prove it hasn't been tampered with, when it was created and by "whom". And who has control, or "custody" of the data, if the custody ever changes. lockb0x ii snot a storage provider or even a storage specification. 
It is a solution for data sovereignty that also provides the basis for Controllable Electronic Records as defined in, and in compliance with, United States Uniform Commercial Code Sections 12, 9, 8, and 1. As revised in 2022 to handle using blockchain in commerce. lockb0x by design supports the standards and ethos of GDPR and personal data ownership and control. 
The lockb0x-protocol Verifier Reference Implementation is under development. Feel free to fork, contribute, and PR!
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

The full technical specification lives in [`spec/v0.0.1-public-draft.md`](spec/v0.0.1-public-draft.md).  
Each section of the spec is broken out into its own file in the `spec/` folder for clarity.  

---

## Contributing

Lockb0x is at an early stage and we welcome feedback, contributions, and discussion.  
- Open issues to suggest improvements or report problems.  
- Submit pull requests to add adapters, verifiers, or clarifications.  
- Join the discussion on standards alignment and compliance use cases.  

---

## License

This project is licensed under the [MIT License](LICENSE), and the Lockb0x Protocol specification and reference implementation are released under the **Apache 2.0 License**.
