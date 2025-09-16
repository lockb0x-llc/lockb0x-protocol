


# 8. Verification (Normative)

Verification is the process of confirming that a Codex Entry is authentic, complete, and compliant with the Lockb0x Protocol.  
Verifiers MUST implement the following steps to ensure integrity, authenticity, and provenance.

---

## 8.1 Verification Steps

A Verifier MUST:

1. **Canonicalization**  
   - Canonicalize the Codex Entry using [RFC 8785] JSON Canonicalization Scheme.  

2. **Signature Validation**  
   - Verify each signature object against its declared `alg` and `kid`.  
   - Ensure the number of valid signatures meets or exceeds the `policy.threshold` (see Section 6).  

3. **Integrity Check**  
   - Validate that the file’s hash matches the `storage.integrity_proof`.  
   - Hash algorithms MUST be collision-resistant (e.g., SHA-256, SHA-3).  

4. **Location Verification**  
   - Confirm that the declared `storage.location` matches the metadata obtained from the adapter.  
   - If notarization is used, validate the provider’s signed attestation.  

5. **Anchor Validation**  
   - Recompute the Codex Entry hash using the declared `hash_alg`.  
   - Confirm that the `anchor.tx_hash` exists on the declared `chain`.  
   - Validate Merkle inclusion proofs or NFT metadata where applicable.  

6. **Encryption and Key Ownership Validation**  
   - Confirm that the declared encryption algorithm matches supported parameters.  
   - Validate that `last_controlled_by` (or equivalent) corresponds to authorized signers under the declared `policy`.  

7. **Revision Chain Traversal**  
   - If `previous_id` is present, verifiers MUST traverse backward through the revision chain to reconstruct full CER history.  
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

---

[RFC 8785]: https://www.rfc-editor.org/rfc/rfc8785