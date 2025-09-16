# 2. Terminology (Normative)

The key words **MUST**, **MUST NOT**, **REQUIRED**, **SHALL**, **SHALL NOT**,  
**SHOULD**, **SHOULD NOT**, **RECOMMENDED**, **MAY**, and **OPTIONAL** in this document  
are to be interpreted as described in [RFC 2119].

- **Codex Entry**: A structured, signed data object that encapsulates integrity proofs, storage references, and anchors. It is the fundamental unit of the Lockb0x Protocol.  
- **Storage Adapter**: A module or function that interfaces with an external storage system (e.g., IPFS, S3, Azure Blob, FTP/SFTP) and produces verifiable proofs of storage.  
- **Anchor**: A blockchain transaction or equivalent immutable record that cryptographically attests to the existence of a Codex Entry at a specific point in time.  
- **Verifier**: A tool or process that checks the validity of a Codex Entry by verifying signatures, integrity proofs, storage claims, and anchors.  
- **Certificate**: A human- or machine-readable document generated from a Codex Entry that provides evidence of integrity, custodianship, and anchoring. Certificates MAY be represented as JSON objects or W3C Verifiable Credentials.  
- **Identity Context**: Identifiers bound to a Codex Entry that describe the controlling party and optional contextual hierarchy (e.g., organization, sub-organization, transaction context).  

[RFC 2119]: https://www.rfc-editor.org/rfc/rfc2119
