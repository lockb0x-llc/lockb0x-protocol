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

## 7.2 Canonical Identity Layout

Codex Entries MUST encode identity using the canonical layout:

```json
"identity": {
  "org": "did:example:org123",
  "project": "did:example:proj456",
  "context": "workorder-789",
  "subject": "did:example:asset999"
}
```

`org` and `context` are REQUIRED; `project` and `subject` are OPTIONAL but strongly RECOMMENDED for full provenance traceability.

A minimal valid identity object includes only the required fields:

```json
"identity": {
  "org": "did:example:org123",
  "context": "workorder-789"
}
```

---

## 7.3 Structural Provenance Hierarchy

- `org` anchors the Codex Entry to the accountable organization. It MUST resolve to a DID or account that can authorize protocol operations.
- `project` narrows provenance to a program, legal entity, or initiative administered by `org`. When present, it MUST also resolve to a DID or account controlled by `org`.
- `context` is REQUIRED and binds the entry to an operational workflow (e.g., work order, case ID, transaction reference). Context identifiers MUST be unique workflow or work order references and SHOULD align with traceability practices defined in Pakana/UCC Article 12.

This hierarchy establishes structural provenance regardless of whether a Codex Entry references natural persons, enterprises, or assets.

---

## 7.4 Subject Binding and Authorization

`subject` identifies the individual, organization, or asset that is the focus of the Codex Entry. It MAY be omitted when the subject is implicit in the workflow context, but when supplied it MUST be a DID or account identifier.

Signatures associated with a Codex Entry (Section 6) MUST authorize the declared hierarchy. Verifiers MUST confirm that signers are permitted to act for the `org`, `project`, and—when present—`subject` values. Additionally, verifiers MUST ensure that keys in `last_controlled_by` (when present in Codex Entries) are associated with the declared `org` or `project` identities.

---

## 7.5 Privacy and Data Minimization

- Identifiers MUST NOT expose sensitive personal information.
- Where possible, pseudonymous or derived identifiers SHOULD be used (e.g., DIDs, hashed identifiers).
- When declaring a `subject`, implementers SHOULD favor privacy-preserving identifiers that can be rotated or revoked.
- Implementations MUST comply with GDPR and equivalent privacy laws when binding natural persons to Codex Entries.

---

[SEP-0020]: https://github.com/stellar/stellar-protocol/blob/master/ecosystem/sep-0020.md
[DID Core]: https://www.w3.org/TR/did-core/
[CAIP-10]: https://github.com/ChainAgnostic/CAIPs/blob/master/CAIPs/caip-10.md