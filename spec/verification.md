# 8. Verification (Normative)

Verification is the process of confirming that a Codex Entry is authentic, complete, and compliant with the Lockb0x Protocol.
Verifiers MUST implement the following steps to ensure integrity, authenticity, and provenance.

The reference implementation provides diagnostic error reporting and covers the full verification pipeline: schema, canonicalization, integrity, signatures, anchor, revision chain, and certificate validation. All steps are covered by deterministic unit and integration tests.

---

## 8.1 Verification Steps

A Verifier MUST:

1. **Schema Validation**

   - Validate the Codex Entry against the JSON Schema defined in Appendix B.
   - MUST reject entries containing any fields not explicitly defined in the schema (unknown or additional fields).
   - MUST validate that all required fields are present and conform to their specified types and constraints.

2. **Canonicalization**

   - Canonicalize the Codex Entry using [RFC 8785] JSON Canonicalization Scheme.

3. **Signature Validation**

   - Verify each signature object against its declared `alg` and `kid`.
   - Ensure the number of valid signatures meets or exceeds the `policy.threshold` (see Section 6).
   - Verifiers MUST confirm that signatures were produced after anchoring and that they cover the `anchor` object.

4. **Integrity Check**

   - Validate that the file’s hash matches the `storage.integrity_proof`.
   - Integrity proofs MUST be expressed as [RFC 6920] ni-URIs only. Provider-specific identifiers (e.g., ipfs:// CIDs, S3 ETags) are not permitted in the canonical Codex Entry representation.
   - Hash algorithms MUST be collision-resistant (e.g., SHA-256, SHA-3).

5. **Location Verification**

   - Confirm that the declared `storage.location` matches the metadata obtained from the adapter.
   - If notarization is used, validate the provider’s signed attestation.

6. **Anchor Validation**

   - Recompute the Codex Entry hash using the declared `hash_alg`.
   - Confirm that the `anchor.tx_hash` exists on the declared `chain`.
   - Validate Merkle inclusion proofs or NFT metadata where applicable.

7. **Encryption and Key Ownership Validation**

   - If the `encryption` object is omitted, the asset is considered plaintext. If present, verifiers MUST validate algorithm, parameters, and ensure `last_controlled_by` matches the signer(s) with effective control.

8. **Revision Chain Traversal**
   - If `previous_id` is present, verifiers MUST traverse backward through the revision chain to reconstruct full CER history.
   - Verifiers MUST validate both `previous_id` and provenance metadata (e.g., `wasDerivedFrom`) for consistency across revisions.
   - Each revision MUST independently pass all verification checks.

---

## 8.2 Verification Outcomes

- If all checks succeed, the Codex Entry MUST be considered valid.
- If any required check fails, the Codex Entry MUST be rejected as invalid.
- Implementations MAY provide diagnostic information on which step(s) failed.

---

## 8.3 Interoperability Considerations

- Verifiers SHOULD be able to operate in offline mode using cached blockchain headers and storage proofs.
- Verifiers MUST reject entries that rely on deprecated algorithms or revoked keys.
- Implementations SHOULD allow pluggable adapters for custom storage systems and blockchains.
- Verifiers MUST validate jurisdiction claims in `storage.location.jurisdiction` against certificate and anchor references.

---

[RFC 8785]: https://www.rfc-editor.org/rfc/rfc8785
[RFC 6920]: https://www.rfc-editor.org/rfc/rfc6920
