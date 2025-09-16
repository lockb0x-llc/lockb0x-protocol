


# Appendix A. Example Flows (Non-Normative)

This appendix illustrates end-to-end flows for creating, anchoring, and verifying Codex Entries.  
These examples are non-normative and are intended to help implementers understand typical usage patterns.

---

## A.1 File → Codex Entry → Stellar Anchor → Verification

1. User selects a file to be recorded.  
2. Storage adapter computes integrity proof (e.g., IPFS CID or S3 ETag).  
3. Codex Entry is created with required fields: `id`, `storage`, `integrity_proof`, `signatures`.  
4. Entry is signed locally with the user’s private key.  
5. Entry is anchored on Stellar by embedding the Codex Entry hash in a transaction memo.  
6. A certificate is generated (JSON, VC, or X.509).  
7. Another party verifies:
   - Recompute file hash and compare with `integrity_proof`.  
   - Validate signatures.  
   - Confirm anchor transaction exists on Stellar.  

---

## A.2 IPFS Example

1. File is added to IPFS, producing a CID (e.g., `Qm...`).  
2. Codex Entry includes:
   - `storage.protocol = "ipfs"`  
   - `storage.integrity_proof = "ni:///sha-256;..."`  
   - `storage.location` with pinning region/jurisdiction.  
3. Entry is signed and anchored on Stellar.  
4. Verifier retrieves file from IPFS using CID, recomputes hash, and validates anchor.  

---

## A.3 S3 Example

1. File is uploaded to S3, producing an ETag checksum.  
2. Codex Entry includes:
   - `storage.protocol = "s3"`  
   - `storage.integrity_proof = ETag`  
   - `storage.location` with `region`, `jurisdiction`, and provider.  
3. Entry is signed and anchored.  
4. Verifier downloads file (or uses metadata), validates hash, and checks anchor.  

---

## A.4 Google Cloud Storage Example

1. File is uploaded to Google Cloud Storage (GCS), yielding object metadata with `crc32c` and `md5Hash` values.
2. Codex Entry includes:
   - `storage.protocol = "gcs"`
   - `storage.integrity_proof = "ni:///sha-256;..."`
   - `storage.location` with bucket location (e.g., `us-central1`), legal jurisdiction (e.g., `US/CA`), and `provider = "Google Cloud"`.
   - Optional recording of the object's `crc32c` for auditors.
3. Entry is signed and anchored on Stellar.
4. Verifier fetches object metadata via the GCS API, recomputes the SHA-256 digest, and validates the anchor.

---

## A.5 Multi-Sig Control Flow

1. Organization defines a policy: 2-of-3 required for encryption/decryption.  
2. Codex Entry includes `encryption.policy` and `encryption.public_keys`.  
3. Two key-holders perform an action (e.g., encrypt file, authorize release).  
4. `last_controlled_by` records which two keys executed control.  
5. Entry is anchored and certificate issued.  
6. Verifier ensures signatures match policy and that control threshold was satisfied.  

---

## A.6 Revision Chain Flow

1. Original file is recorded as Codex Entry `id=UUID1`.  
2. Later, a revised version is created as Codex Entry `id=UUID2`, with `previous_id=UUID1`.  
3. A second revision produces Codex Entry `id=UUID3`, with `previous_id=UUID2`.  
4. Verifier reconstructs history by traversing `previous_id` links: UUID3 → UUID2 → UUID1.  
5. Each entry is independently verified, producing a complete audit trail.  

---

These example flows show how the Lockb0x Protocol can be applied across storage backends, multi-sig contexts, and revision chains.