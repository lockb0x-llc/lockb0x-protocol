# Appendix D: Reference Implementations (Informative)

The Lockb0x Protocol is intentionally implementation‑neutral (see Section 1.3).
This appendix provides **non‑normative** examples that demonstrate how the
protocol can be realized across different identity and storage ecosystems. The
examples illustrate that the protocol is compatible with both federated
identity providers and public blockchain networks. These implementations are
provided for illustration only and do not constitute a central registry. Each
provider is responsible for publishing its own descriptor and maintaining its
reference code.

## D.1 Summary Matrix

| Implementation | Identity Source                     | Storage Adapter                    | Cover Signature              | Signature Algorithms |
| -------------- | ----------------------------------- | ---------------------------------- | ---------------------------- | -------------------- |
| **Google**     | Google OIDC ID Token (JWS)          | Google Drive (Service Account JWS) | WebAuthn Passkey (ES256)     | RS256, ES256         |
| **Ethereum**   | EIP‑4361 Sign‑In with Ethereum      | IPFS / Filecoin                    | Wallet signature (secp256k1) | ECDSA secp256k1      |
| **Polkadot**   | Substrate account (sr25519)         | On‑chain remark + IPFS             | sr25519 signature            | sr25519              |
| **Microsoft**  | Microsoft Entra ID OIDC Token (JWS) | OneDrive via Graph API (JWS)       | WebAuthn Passkey (ES256)     | RS256, ES256         |

### D.2 Guidance on Providers

These reference implementations are illustrative and may evolve independently.
They demonstrate that the Lockb0x Protocol can function in diverse ecosystems:

- **Federated identity providers** (e.g., Google, Microsoft) rely on OpenID
  Connect ID tokens for the identity anchor and use cloud storage services for
  the storage anchor.
- **Public blockchains** (e.g., Ethereum, Polkadot) use account signatures or
  structured messages for the identity anchor and on‑chain data (or IPFS) for
  the storage anchor.

Each implementation publishes a **self‑describing provider descriptor**
document that conforms to the schema defined in
`schema/provider-descriptor.schema.json`. Descriptors SHOULD be signed by the
implementer and MUST be hosted at a stable URI or decentralized address. The
Lockb0x project maintains example descriptors for the implementations listed
above, but inclusion in this appendix is not required for compliance.

Implementers are free to adopt, extend, or replace these examples as long as
their implementations conform to the normative requirements in Section 16.
