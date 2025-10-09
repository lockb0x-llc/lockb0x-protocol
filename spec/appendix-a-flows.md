# Appendix A. Example Flows (Non-Normative)

This appendix illustrates end-to-end flows for creating, anchoring, and verifying Codex Entries.
These examples are non-normative and are intended to help implementers understand typical usage patterns.
Note: the "other" party that is a verifier can in some in some scenarios could be a smart-contract or other automated process.

---

## A.1 File → Codex Entry → Stellar Anchor → Verification

1. User selects a file to be recorded.
2. Storage adapter computes the canonical integrity proof by deriving an RFC 6920 ni-URI (transforming any backend-specific values such as IPFS CIDs or S3 ETags).
3. Codex Entry is created with required fields: `id`, `storage`, `integrity_proof` (no signatures yet).

```json
{
  "id": "UUID1",
  "storage": { "protocol": "ipfs", "integrity_proof": "ni:///sha-256;..." },
  "identity": {
    "org": "Org123",
    "process": "Onboarding2025",
    "artifact": "EmployeeHandbook-v1"
  }
}
```

4. Codex Entry is anchored on Stellar by embedding the Codex Entry hash in a transaction memo.
5. Codex Entry is signed locally with the user’s private key after anchoring.
6. A Certificate of Verification is generated (JSON, VC, or X.509).
7. Another party verifies:
   - Recompute file hash and compare with `integrity_proof`.
   - Validate signatures.
   - Confirm anchor transaction exists on Stellar.

---

## A.2 IPFS Example

1. File is added to IPFS, producing a CIDv0 (e.g., `QmT78zSuBmuS4z925WZfrqQ1qHaJ56DQaTfyMUF7F8ff5o`).
2. Codex Entry includes:
   - `storage.protocol = "ipfs"`
   - `storage.integrity_proof = "ni:///sha-256;b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9"`
   - `storage.location` with pinning region/jurisdiction.

```json
{
  "id": "UUID1",
  "storage": {
    "protocol": "ipfs",
    "integrity_proof": "ni:///sha-256;b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9"
  },
  "identity": {
    "org": "Org123",
    "process": "Onboarding2025",
    "artifact": "EmployeeHandbook-v1"
  }
}
```

**Note:** The original IPFS CID `QmT78zSuBmuS4z925WZfrqQ1qHaJ56DQaTfyMUF7F8ff5o` contained a SHA-256 multihash which was extracted and canonically mapped to the ni-URI format. The adapter retains the ability to resolve the file using the original CID for IPFS tooling compatibility.

3. Codex Entry is anchored on Stellar.
4. Signatures are generated after anchoring.
5. Verifier retrieves file from IPFS using CID, recomputes hash, and validates anchor.

---

## A.3 S3 Example

1. File is uploaded to S3, producing an ETag checksum (e.g., `"d41d8cd98f00b204e9800998ecf8427e"`).
2. Codex Entry includes:
   - `storage.protocol = "s3"`
   - `storage.integrity_proof = "ni:///md5;1B2M2Y8AsgTpgAmY7PhCfg"` (adapter transforms the S3 ETag using canonicalization rules - ETag was MD5, so mapped to `ni:///md5;...`)
   - `storage.location` with `region`, `jurisdiction`, and provider.

```json
{
  "id": "UUID2",
  "storage": {
    "protocol": "s3",
    "integrity_proof": "ni:///md5;1B2M2Y8AsgTpgAmY7PhCfg"
  },
  "identity": {
    "org": "Org123",
    "process": "Onboarding2025",
    "artifact": "EmployeeHandbook-v1"
  }
}
```

**Note:** The original S3 ETag `d41d8cd98f00b204e9800998ecf8427e` was recognized as a valid MD5 hash and canonically mapped to the ni-URI format. For multipart uploads or unrecognized ETags, the adapter would download the file content and compute SHA-256 instead.

3. Codex Entry is anchored on Stellar.
4. Signatures are generated after anchoring.
5. Verifier downloads file (or uses metadata), validates hash, and checks anchor.

---

## A.4 Google Cloud Storage Example

1. File is uploaded to Google Cloud Storage (GCS), yielding object metadata with `crc32c` and `md5Hash` values.
2. Codex Entry includes:
   - `storage.protocol = "gcs"`
   - `storage.integrity_proof = "ni:///sha-256;..."`
   - `storage.location` with bucket location (e.g., `us-central1`), legal jurisdiction (e.g., `US/CA`), and `provider = "Google Cloud"`.
   - Optional recording of the object's `crc32c` for auditors.

```json
{
  "id": "UUID3",
  "storage": { "protocol": "gcs", "integrity_proof": "ni:///sha-256;..." },
  "identity": {
    "org": "Org123",
    "process": "Onboarding2025",
    "artifact": "EmployeeHandbook-v1"
  }
}
```

3. Codex Entry is anchored on Stellar.
4. Signatures are generated after anchoring.
5. Verifier fetches object metadata via the GCS API, recomputes the SHA-256 digest, and validates the anchor.

---

## A.5 Solid Pod Example

1. File is uploaded to a Solid Pod at a specific HTTPS URI (e.g., `https://alice.solidcommunity.net/private/myfile.txt`).
2. Codex Entry includes:
   - `storage.protocol = "solid"`
   - `storage.integrity_proof = "ni:///sha-256;..."` (computed by downloading the file and hashing its contents)
   - `storage.location` with the Pod URL, legal jurisdiction, and provider (e.g., `provider = "Solid Community Pod"`).

```json
"identity": {
  "org": "Org456",
  "process": "ResearchProject",
  "artifact": "Dataset2025",
  "webid": "https://alice.solidcommunity.net/profile/card#me"
}
```

3. Codex Entry is anchored on Stellar.
4. Signatures are generated after anchoring.
5. Verifier retrieves the file from the Solid Pod using the HTTPS URI, recomputes the hash, validates the anchor on Stellar, and checks the WebID binding/authorization.

---

## A.6 Multi-Sig Control Flow

1. Organization defines a policy: 2-of-3 required for encryption/decryption.
2. Codex Entry includes `encryption.policy` and `encryption.public_keys`.
3. Two key-holders perform an action (e.g., encrypt file, authorize release).
4. `last_controlled_by` MUST record the actual key-holders responsible for the last control event in multi-sig.
5. Codex Entry is anchored and Certificate of Verification issued.
6. Verifier ensures signatures match policy and that control threshold was satisfied.

---

## A.7 Revision Chain Flow

1. Original file is recorded as Codex Entry `id=UUID1`.
2. Later, a revised version is created as Codex Entry `id=UUID2`, with `previous_id=UUID1` and provenance assertion `wasDerivedFrom: UUID1`.
3. A second revision produces Codex Entry `id=UUID3`, with `previous_id=UUID2` and `wasDerivedFrom: UUID2`.
4. Verifier reconstructs history by traversing both `previous_id` links and provenance assertions: UUID3 → UUID2 → UUID1.
5. Verifier checks consistency of encryption metadata and `last_controlled_by` fields across revisions, if present.
6. Each Codex Entry is independently verified, producing a complete audit trail.

---

## A.8 Full Protocol Pipeline Example

1. Store file in IPFS (using IpfsStorageAdapter) → obtain CID and ni-URI integrity proof
2. Create Codex Entry with required fields (id, storage, integrity_proof, identity)
3. Canonicalize entry (RFC 8785 JCS)
4. Anchor entry on Stellar (mock/in-memory for now)
5. Sign entry with user's private key (Ed25519, ES256K, RS256)
6. Generate certificate (JSON, VC, or X.509)
7. Verify: recompute file hash, validate signatures, confirm anchor transaction on Stellar, traverse revision chain if present

Platform-specific notes: Secp256k1 and X.509 have limitations on macOS/Linux; see code and tests for details.

---

---

## A.9 Ethereum Anchor and Verifier (EVM-compatible)

**Purpose:** Record and verify Lockb0x Codex Entry digests on Ethereum or compatible EVM networks.

1. Compute canonical SHA-256 digest of target media or CER JSON.
2. Construct ni-URI per RFC 6920 (`ni:///sha-256;<b64url>`).
3. Invoke `Lockb0x_Anchor_Eth.anchor(digest, metadata)` on-chain, where:
   - `digest` is the bytes32 hash (SHA-256 or Keccak256)
   - `metadata` is a JSON string (can include cerId, niUri, externalRef, alg, etc.)
4. Record emitted `Anchored` event with hash and metadata.
5. To verify:
   - Retrieve anchor by hash via `getAnchor(digest)`.
   - Recompute digest off-chain per Lockb0x canonical rules.
   - Compare on-chain digest vs. recomputed digest.
   - If match → integrity verified.

**Example JSON Test Vector:**

```json
{
  "flow": "ethereum-anchor",
  "input": {
    "digest": "0x61dfaa61c9e6b3a8d9ec8c28b90e6931d7c43ed9869d3a13ec099d19b5a74e3a",
    "niUri": "ni:///sha-256;Yd-qYcnms6jZ7Iwo2Q5pMdfEPtrGnToT7AmdGbWnTjo",
    "cerId": "0x0000000000000000000000000000000000000000000000000000000000000000",
    "externalRef": "ipfs://bafybeigdyr...",
    "alg": "SHA256"
  },
  "output": {
    "hash": "0x61dfaa61c9e6b3a8d9ec8c28b90e6931d7c43ed9869d3a13ec099d19b5a74e3a",
    "metadata": "{\"cerId\":\"0x000...000\",\"niUri\":\"ni:///sha-256;Yd-qY...\",\"externalRef\":\"ipfs://bafybeigdyr...\",\"alg\":\"SHA256\"}",
    "transactionHash": "0xe56a....",
    "timestamp": 1738968100,
    "submitter": "0xAEc9...8b3"
  }
}
```

**Notes:**

- The contract does not use anchorId or getById; the hash is the unique key.
- Metadata should be a JSON string containing all relevant fields.
- Verification is always by hash, matching contract and SDK logic.

These example flows show how the Lockb0x Protocol can be applied across storage backends, multi-sig contexts, revision chains, and EVM-compatible blockchains.
