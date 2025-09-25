# 7. Identity (Normative)

Identity fields in a Codex Entry bind the record to individuals, organizations, or assets.  
This allows verifiers to establish accountability, custodianship, and legal recognition under frameworks such as GDPR (EU) and UCC Section 12 (US).

## 7.0 Terminology and Contextual Mappings

**Canonical Terminology:**
- **Organization**: An entity capable of acting in the protocol.
- **Process**: A subdivision, project, or role accountable within an Organization.
- **Artifact**: A digital object or record, possibly a Controllable Electronic Record under UCC Article 12.
- **Subject**: The individual, entity, or asset that is the focus of the Codex Entry.

**Contextual Mappings:**
The following mappings illustrate how the abstract IETF standards terminology applies to real-world implementations:

- **In Pakana**: Organization → Organization, Project → Process, WorkOrder → Artifact.
- **In UCC**: Debtor/Secured Party → Organization, CER/Collateral → Artifact.
- **In GDPR**: Data Subject → Subject, Personal Data → Artifact (when data is represented).

---

## 7.1 Acceptable Identifiers

Codex Entries MUST use one of the following identifier formats:

- **Stellar Account Address** — in [SEP-0020] format (`G...` public key).  
- **W3C Decentralized Identifier (DID)** — conforming to [DID Core].  
- **Other Blockchain Account Identifiers** — following [CAIP-10] for cross-chain identities.  
- **Solid WebID** — conforming to [Solid WebID].

Identifiers MUST be unique, resolvable, and verifiable against their respective network or DID method.

---

## 7.2 Canonical Identity Layout

Codex Entries MUST encode identity using the canonical layout:

```json
"identity": {
  "org": "did:example:org123",
  "process": "did:example:proj456",
  "artifact": "workorder-789",
  "subject": "did:example:asset999"
}
```

`org`, `process`, and `artifact` are REQUIRED; `subject` is OPTIONAL but strongly RECOMMENDED for full provenance traceability.

A minimal valid identity object includes only the required fields:

```json
"identity": {
  "org": "did:example:org123",
  "process": "did:example:proj456", 
  "artifact": "workorder-789"
}
```

An example using a Solid WebID for `subject`:

```json
"identity": {
  "org": "did:example:org123",
  "process": "did:example:proj456",
  "artifact": "workorder-789",
  "subject": "https://user.solidcommunity.net/profile/card#me"
}
```

---

## 7.3 Structural Provenance Hierarchy

- `org` anchors the Codex Entry to the accountable organization. It MUST resolve to a DID or account that can authorize protocol operations.
- `process` narrows provenance to a program, legal entity, or initiative administered by `org`. When present, it MUST also resolve to a DID or account controlled by `org`.
- `artifact` is REQUIRED and binds the entry to an operational workflow (e.g., work order, case ID, transaction reference). Artifact identifiers MUST be unique workflow or work order references and SHOULD align with traceability practices defined in Pakana/UCC Article 12.

This hierarchy establishes structural provenance regardless of whether a Codex Entry references natural persons, enterprises, or assets.

When Solid WebIDs are used, the `org`, `process`, or `subject` MAY resolve to a WebID profile document and associated access control metadata.

---

## 7.4 Subject Binding and Authorization

`subject` identifies the individual, organization, or asset that is the focus of the Codex Entry. It MAY be omitted when the subject is implicit in the workflow context, but when supplied it MUST be a DID or account identifier.

Signatures associated with a Codex Entry (Section 6) MUST authorize the declared hierarchy. Verifiers MUST confirm that signers are permitted to act for the `org`, `process`, and—when present—`subject` values. Additionally, verifiers MUST ensure that keys in `last_controlled_by` (when present in Codex Entries) are associated with the declared `org` or `process` identities.

Verifiers MUST confirm that Solid WebIDs and associated access control lists (WAC/ACP) authorize the claimed identities.

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
[Solid WebID]: https://solidproject.org/