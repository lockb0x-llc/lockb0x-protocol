# 5. Anchoring (Normative)

Anchoring is the process of linking a Codex Entry to an immutable, time-stamped record on a blockchain or equivalent ledger.  
This provides cryptographic proof that the Codex Entry existed at or before a specific point in time.

---

## 5.1 Anchoring Requirements

- Every Codex Entry MUST include an `anchor` object when immutability and timestamp proofs are required.  
- Anchors MUST be created before signatures are applied; the final signed Codex Entry MUST include the anchor object.  
- Anchors MUST reference:
  - `chain`: a [CAIP-2] compliant blockchain identifier.
  - `anchor_ref`: the transaction hash containing the anchor payload.
  - `hash_alg`: the algorithm used to produce the Codex Entry hash, which MUST correspond to the integrity proof scheme (ni-URI) declared in the Codex Entry.
- Anchors MAY include a `token_id` when referencing a specific on-chain asset; this field is REQUIRED for NFT-based anchors.
- NFT-based anchors MUST also include a `contract_address` field to unambiguously identify the smart contract hosting the NFT.

- The Codex Entry content MUST be hashed and included in the blockchain transaction, either directly or via a Merkle root.  
- Anchors MUST be reproducible: the same Codex Entry must yield the same anchor hash when re-hashed.  

---

## 5.2 Supported Blockchains

Implementations MUST support anchoring on the Stellar network in the reference implementation.  
Other chains MAY be supported, including but not limited to:

- Ethereum / EVM-compatible chains  
- Avalanche (C-chain and subnets)  
- Bitcoin (via OP_RETURN)  
- Hyperledger-based ledgers  

### 5.2.1 Stellar Network Constraints

Stellar transaction memos are limited to 28 bytes maximum. Due to this constraint:

- The full SHA-256 hash of a Codex Entry (32 bytes) cannot fit directly in a Stellar memo field.
- Implementations MUST use a shortened identifier approach for Stellar anchoring while maintaining uniqueness and traceability.
- The recommended approach is to combine an MD5 hash (16 bytes) of the Codex Entry with the signing account's public key to generate a unique 28-byte identifier for the memo field.
- The canonical SHA-256 hash MUST still be recorded in the Codex Entry's `anchor.hash_alg` field and used for verification purposes.
- Verifiers MUST be able to reconstruct the memo identifier from the Codex Entry and validate it against the on-chain transaction memo.

---

## 5.3 Transaction Payloads

- The blockchain transaction payload MUST include either:
  - The full hash of the Codex Entry, OR  
  - A Merkle root representing one or more Codex Entries.  

- The payload content MUST map back to the Codex Entry’s `integrity_proof` field.  
- Payloads SHOULD use efficient encoding formats (e.g., hex or base58).  
- Anchors SHOULD minimize on-chain storage, relying on off-chain Codex Entries for details.  

### 5.3.1 NFT Anchoring

Implementations MAY use Non-Fungible Tokens (NFTs) as anchor carriers.
In this model, the Codex Entry hash is embedded in the NFT’s metadata or in the minting transaction payload.
When an NFT is used, the `token_id` field in the anchor object becomes mandatory and MUST reference the on-chain token identifier.
Additionally, the `contract_address` field becomes mandatory to unambiguously identify which smart contract hosts the NFT.

Anchors using NFTs MUST declare:

- `chain`: a [CAIP-2] blockchain identifier for the NFT’s chain.
- `anchor_ref`: the transaction hash of the NFT minting or transfer that includes the Codex Entry hash.
- `hash_alg`: the algorithm used to produce the Codex Entry hash.
- `token_id`: the unique identifier of the NFT on-chain.
- `contract_address`: the smart contract address hosting the NFT.

Examples:

Non-NFT anchor:

```json
"anchor": {
  "chain": "eip155:1",
  "anchor_ref": "0xdef456...",
  "hash_alg": "SHA256"
}
```

NFT anchor:

```json
"anchor": {
  "chain": "eip155:1",
  "anchor_ref": "0xabc123...",
  "token_id": "123456789",
  "contract_address": "0x1234567890abcdef1234567890abcdef12345678",
  "hash_alg": "SHA256"
}
```

Verifiers MUST confirm:

1. The NFT exists at the declared `token_id` and `contract_address` on the declared `chain`.  
2. The NFT’s metadata or minting transaction contains the declared Codex Entry hash.  
3. The transaction is final and timestamped consistently with the blockchain’s block time.  

NFT anchoring provides an additional layer of provenance, as the NFT itself can be traded or referenced as the canonical representation of the Codex Entry.

---

## 5.4 Verification Rules

A Verifier MUST:

1. Recompute the Codex Entry hash using the declared `hash_alg`.  
2. Confirm the transaction exists on the declared `chain` by querying a trusted node or API.  
3. Confirm the `anchor_ref` contains the declared Codex Entry hash (or Merkle inclusion proof).  
4. Validate timestamp consistency with the blockchain’s block time.  
5. If anchor verification fails, the Codex Entry MUST be rejected and no Certificate of Verification may be issued.

When Codex Entries are bound to multi-sig identities, Verifiers MUST ensure the `last_controlled_by` record aligns with the anchored transaction signers.

If any of these checks fail, the anchor MUST be considered invalid.

---

## References

[CAIP-2]: https://github.com/ChainAgnostic/CAIPs/blob/master/CAIPs/caip-2.md