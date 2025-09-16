# 1. Scope (Non-Normative)

The Lockb0x Protocol defines a verifiable, storage-agnostic mechanism for proving the existence, integrity, and custodianship of digital data. It is **not a storage protocol**; rather, it operates as a verification and provenance layer that can sit atop existing storage and blockchain systems.

The protocol addresses the following objectives:

- **Data Sovereignty**: enabling organizations and individuals to assert control over where and how their data is stored, independent of the underlying provider.  
- **Compliance & Auditability**: providing a verifiable chain of custody suitable for regulatory frameworks such as GDPR in the EU and UCC Section 12 in the United States.  
- **Cross-Organization Trust**: allowing multiple independent parties to share and verify data without requiring a single trusted intermediary.  

The Lockb0x Protocol does not replace existing standards for storage, encryption, or identity. Instead, it builds upon them by defining a **Codex Entry** abstraction that links together storage proofs, cryptographic signatures, and blockchain anchors in a consistent, verifiable format.