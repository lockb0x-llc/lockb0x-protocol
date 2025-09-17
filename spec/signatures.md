# 6. Signatures (Normative)

Codex Entries MUST be signed to ensure authenticity and non-repudiation.  
Signatures MUST be created **after anchoring** so they cover the full Codex Entry including the anchor object.  
Signatures provide cryptographic proof of authorship and are a core requirement for verification.

---

## 6.1 Signature Format

- Signatures MUST conform to either [JOSE JWS] (JSON Web Signature, RFC 7515) or [COSE Sign1] (CBOR Object Signing and Encryption, RFC 8152).  
- Each signature object MUST include:
  - `alg`: the algorithm used (e.g., `EdDSA`, `ES256K`, `RS256`).  
  - `kid`: key identifier (MUST be resolvable to a Stellar account, W3C DID, or equivalent).  
  - `signature`: the base64url-encoded or CBOR-encoded signature bytes.  
- Optional header parameters (e.g., `crit`, `x5c`) MAY be included when needed for interoperability.

---

## 6.2 Multiple Signatures

- A Codex Entry MAY include multiple signatures.  
- Multiple signatures are REQUIRED for multi-sig contexts defined in the `encryption.policy`.  
- Each signer MUST sign the canonicalized Codex Entry payload.  
- In multi-sig contexts, `last_controlled_by` MUST reflect the key(s) that actually signed most recently.  
- Verification MUST succeed only if the required threshold of signatures is present and valid.

---

## 6.3 Canonicalization

- The Codex Entry MUST be canonicalized before signing and verification.  
- Implementations SHOULD use [RFC 8785] JSON Canonicalization Scheme (JCS).  
- Canonicalization ensures that field ordering, whitespace, and encoding do not affect the signature outcome.  

---

## 6.4 Verification Rules

A Verifier MUST:

1. Canonicalize the Codex Entry according to [RFC 8785].  
2. Validate each signature against the declared `alg` and `kid`.  
3. Confirm that the signer identity (`kid`) matches an expected key in the Codex Entry (e.g., `identity.org`, `encryption.public_keys`).  
4. Confirm that signatures cover the `anchor` object and were produced after anchoring.  
5. In multi-sig contexts, confirm that the number of valid signatures meets the declared `policy.threshold`.  

If any of these checks fail, the signature validation MUST be considered invalid.

---

## 6.5 Acceptable Algorithms

The following algorithms MUST be supported in the reference implementation:

- **EdDSA (Ed25519)** — REQUIRED  
- **ES256K (ECDSA secp256k1)** — REQUIRED for blockchain compatibility  
- **RS256 (RSA SHA-256)** — RECOMMENDED for interoperability  

Algorithms MUST follow current NIST and IETF cryptographic recommendations.  
The spec also includes AES-GCM and ChaCha20-Poly1305 as encryption algorithms for encryption contexts; however, these are not signature algorithms and are referenced here for completeness.

Implementations MAY support additional algorithms provided they meet current NIST and IETF cryptographic recommendations.

---

## 6.6 Non-Repudiation

- Signatures MUST bind the signer’s identity to the Codex Entry at the time of signing.  
- Once recorded, a valid signature MUST be treated as legally binding evidence of authorship and custodianship under applicable frameworks (e.g., UCC Section 12, eIDAS).  
- Revocation of keys does not retroactively invalidate signatures, but future Codex Entries MUST NOT accept revoked keys.

---

[JOSE JWS]: https://www.rfc-editor.org/rfc/rfc7515
[COSE Sign1]: https://www.rfc-editor.org/rfc/rfc8152
[RFC 8785]: https://www.rfc-editor.org/rfc/rfc8785