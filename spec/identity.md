


# 7. Identity (Normative)

Identity fields in a Codex Entry bind the record to individuals, organizations, or assets.  
This allows verifiers to establish accountability, custodianship, and legal recognition under frameworks such as GDPR (EU) and UCC Section 12 (US).

---

## 7.1 Acceptable Identifiers

Codex Entries MUST use one of the following identifier formats:

- **Stellar Account Address** — in [SEP-0020] format (`G...` public key).  
- **W3C Decentralized Identifier (DID)** — conforming to [DID Core].  
- **Other Blockchain Account Identifiers** — following [CAIP-10] for cross-chain identities.  

Identifiers MUST be unique, resolvable, and verifiable against their respective network or DID method.

---

## 7.2 Identity Contexts

A Codex Entry MAY include multiple identity contexts:

- `individual`: an identified natural person, represented by a public key or DID.  
- `entity`: an organization or legal person, represented by a Stellar account, DID, or legal identifier.  
- `asset`: a specific asset or digital object, identified by a DID or registry ID.  

These contexts allow flexible mapping between technical and legal accountability.  

---

## 7.3 Binding Rules

- Each Codex Entry MUST declare at least one identity context (`individual` or `entity`).  
- An `asset` identity MAY be included when the entry refers to a specific digital asset or resource.  
- Identity contexts MUST be bound to the Codex Entry via cryptographic signatures (see Section 6).  
- If multiple contexts are present, verifiers MUST confirm that the declared signers are authorized to act for each declared context.

---

## 7.4 Hierarchical Contexts

Codex Entries MAY define hierarchical identity relationships to support organizational workflows. For example:

```json
"identity": {
  "entity": "did:example:org123",
  "project": "did:example:project456",
  "transaction_context": "uuid-7890"
}
```

This allows implementers to represent organizations, sub-projects, and compliance or transaction contexts (e.g., work orders, case IDs, contracts).  
Such hierarchical contexts are OPTIONAL but RECOMMENDED for compliance frameworks requiring traceability.

---

## 7.5 Privacy and Data Minimization

- Identifiers MUST NOT expose sensitive personal information.  
- Where possible, pseudonymous or derived identifiers SHOULD be used (e.g., DIDs, hashed identifiers).  
- Implementations MUST comply with GDPR and equivalent privacy laws when binding natural persons to Codex Entries.

---

[SEP-0020]: https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0020.md
[DID Core]: https://www.w3.org/TR/did-core/
[CAIP-10]: https://github.com/ChainAgnostic/CAIPs/blob/master/CAIPs/caip-10.md