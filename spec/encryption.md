

# 6. Encryption (Normative)

Encryption of assets in Codex Entries is OPTIONAL. If assets are stored in plaintext, the `encryption` object MUST be omitted from the entry. If encryption is used, the `encryption` object MUST be present and MUST include the following fields:

- `algorithm`: Specifies the encryption algorithm used. MUST be a recognized, secure, and standardized algorithm such as `"AES-256-GCM"` or `"ChaCha20-Poly1305"`. Implementations SHOULD reference [NIST recommendations](https://csrc.nist.gov/publications/detail/sp/800-38d/final) for authenticated encryption.
- `key_ownership`: Describes how encryption keys are managed. Valid values are:
    - `"org-managed"`: Keys are controlled by a single organization.
    - `"multi-sig"`: Keys are controlled by multiple parties according to a threshold or quorum policy.
    - `"custodian"`: Keys are held by a designated third-party custodian.
- `policy`: (Required if `key_ownership` is `"multi-sig"`) Describes the threshold or quorum rules for decryption (e.g., `"2-of-3"`).
- `last_controlled_by`: MUST list the key identifiers (e.g., public key fingerprints) responsible for the most recent control or signing event in a multi-sig context. For `"org-managed"`, MAY be a single key identifier. MUST be omitted if no encryption is used.

## Examples

### Plaintext Asset (No Encryption)
```json
{
  "asset_id": "abc123",
  "data": "plain data"
  // No "encryption" object
}
```

### Single-Key Encryption
```json
{
  "asset_id": "enc456",
  "data": "<encrypted_blob>",
  "encryption": {
    "algorithm": "AES-256-GCM",
    "key_ownership": "org-managed",
    "last_controlled_by": ["pubkey1"]
  }
}
```

### Multi-Sig Encryption with Threshold and last_controlled_by
```json
{
  "asset_id": "enc789",
  "data": "<encrypted_blob>",
  "encryption": {
    "algorithm": "ChaCha20-Poly1305",
    "key_ownership": "multi-sig",
    "policy": "2-of-3",
    "last_controlled_by": ["pubkey2", "pubkey3"]
  }
}
```

> **Note:** The `encryption` metadata is provided solely for custody and compliance tracking. It does **not** replace or serve as cryptographic validation of the entry's confidentiality or authenticity. All cryptographic operations MUST be verified independently of metadata.