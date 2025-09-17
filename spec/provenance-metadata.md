


# 10. Provenance & Metadata (Normative + Informative)

Provenance and metadata provide the contextual information necessary to interpret **Codex Entries** in compliance, audit, and interoperability settings.  
This section defines the requirements for recording provenance, mapping to existing standards, and ensuring metadata consistency.

---

## 10.1 Provenance Model

- Provenance MUST be expressed in a structured, machine-readable format.  
- Implementations SHOULD align with the [W3C PROV-DM] data model for representing entities, activities, and agents.  
- Provenance MAY be serialized in JSON-LD to allow semantic linking with external ontologies.  

### Core Provenance Assertions

Each **Codex Entry** MUST support the following provenance assertions:

- **wasGeneratedBy**: the process or workflow that produced the entry.  
- **wasAttributedTo**: the individual or entity responsible for the entry.  
- **wasDerivedFrom**: prior **Codex Entries** or source materials.  
- **wasAnchoredIn**: the blockchain or ledger that anchored the entry.  

#### Revision Linkage Consistency

If a **Codex Entry** is a revision (i.e., has a `previous_id` referencing a prior entry), its provenance assertions MUST include a linkage to the previous entry using both the `previous_id` field and a corresponding `wasDerivedFrom` assertion. This ensures consistency with Section 3 (Data Model) and maintains an unbroken, verifiable chain of revisions in the provenance metadata.

---

## 10.2 Metadata Extensions

- **Codex Entries** MUST include metadata describing storage, encryption, and anchoring.  
- If an `encryption` object is present in the **Codex Entry**, the metadata MUST accurately reflect its structure and content.  
- If no `encryption` object is present, the metadata MUST omit this field.  
- Additional metadata MAY be included using extension fields.  
- Extensions SHOULD follow JSON-LD conventions to ensure semantic interoperability.  

Recommended metadata vocabularies:

- **Data Privacy Vocabulary (DPV)** for privacy and legal context (GDPR, CCPA, etc.).  
- **Dublin Core (DCMI)** for general-purpose descriptive metadata.  
- **Schema.org** terms for integration with web data ecosystems.  

---

## 10.3 Revision Tracking

- **Codex Entries** MUST support revision tracking via the `previous_id` field.  
- Provenance metadata MUST link revisions to establish a full lineage of **Codex Entries**.  
- Each revision MUST independently validate per Sections 6–8, while also preserving the revision chain.  
- Each revision of a **Codex Entry** MUST preserve both the `previous_id` field and a provenance assertion (such as `wasDerivedFrom`) referencing the prior entry, ensuring that both cryptographic and semantic lineage are maintained throughout the revision history.

---

## 10.4 Jurisdictional Metadata

- **Codex Entries** MAY include jurisdictional metadata (e.g., country codes, regulatory domains).  
- Jurisdictional metadata SHOULD be tied to the `storage.location.jurisdiction` field within the entry.  
- Implementations MUST ensure that any jurisdictional claims are cryptographically verifiable, for example by validating against anchors or certificates.  
- Jurisdictional metadata SHOULD follow ISO 3166-1 alpha-2 country codes and relevant regulatory identifiers.  

---

## 10.5 Informative Use Cases

- **Compliance Audits**: Regulators can verify that records comply with GDPR by inspecting DPV-linked metadata.  
- **Cross-Border Data Flow**: Organizations can assert where data is stored and anchored, aiding compliance with EU/US data sovereignty requirements.  
- **Digital Supply Chains**: Metadata can track asset custody across organizations, with provenance ensuring accountability at each step.  
- **Multi-Sig Governance**: Provenance metadata can demonstrate which keys (using the `last_controlled_by` field) exercised control at each revision of a **Codex Entry**, supporting accountability and auditability in shared custody or multi-signature governance scenarios.  

---

[W3C PROV-DM]: https://www.w3.org/TR/prov-dm/
[DPV]: https://w3c.github.io/dpv/
[DCMI]: https://www.dublincore.org/specifications/dublin-core/dcmi-terms/
[Schema.org]: https://schema.org/
[ISO 3166-1]: https://www.iso.org/iso-3166-country-codes.html