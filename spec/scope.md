The Lockb0x Protocol defines a verifiable, storage‑agnostic mechanism for
proving the existence, integrity, and custodianship of digital data. It is **not
a storage protocol**; rather, it operates as a verification and provenance
layer that can sit atop existing storage and blockchain systems.

The protocol addresses the following objectives, and is based on compliance
with UCC Article 12 revisions (US) and EU Data Sovereignty policies:

- **Data Sovereignty**: enabling organizations and individuals to assert
  control over where and how their data is stored, independent of the underlying
  provider.
- **Compliance & Auditability**: originating in the need to address UCC
  Article 12 (Controllable Electronic Records) in the United States and EU Data
  Sovereignty regulations, while also providing a verifiable chain of custody
  suitable for GDPR and related digital asset regulations.
- **Cross‑Organization Trust**: allowing multiple independent parties to
  share and verify data without reliance on centralized authorities.

The Lockb0x Protocol does not replace existing standards for storage,
encryption, or identity. Instead, it builds upon them by defining a **Codex
Entry** abstraction that unifies storage proofs, cryptographic signatures,
organization/process/artifact identity bindings, and blockchain anchors in a
consistent, verifiable format.

Lockb0x is intentionally **implementation‑neutral**. The protocol defines
only the semantics of a Codex Entry and the associated verification process.
It does not prescribe particular identity or storage systems, nor does
it depend on any specific blockchain or anchoring network. Reference
implementations (see Appendix D) demonstrate that the protocol can be used
across federated identity providers (e.g., Google, Microsoft), public
blockchains (e.g., Ethereum, Polkadot), and decentralized storage networks
(e.g., IPFS). These examples are informative rather than prescriptive.
Implementers are free to choose any combination of identity services,
storage adapters, and cover signature mechanisms, provided they produce
verifiable signatures and conform to the normative requirements defined
elsewhere in this specification.

                  ┌──────────────────────────────────────┐
                  │           Lockb0x Codex              │
                  │   (canonical JSON envelope, ni)      │
                  └───────────────┬──────────────────────┘
                                  │
            ┌─────────────────────┼──────────────────────────┐
            │                     │                          │
            ▼                     ▼                          ▼

┌────────────────┐ ┌──────────────────┐ ┌──────────────────┐
│ Google Anchor │ │ Ethereum Anchor │ │ Microsoft Anchor│
│ • OIDC IDToken │ │ • EIP-4361 Msg │ │ • Entra ID Token │
│ • Drive JWS │ │ • IPFS/Filecoin │ │ • OneDrive JWS │
│ • WebAuthn Sig │ │ • Wallet Sig │ │ • WebAuthn Sig │
└────────────────┘ └──────────────────┘ └──────────────────┘
│ │ │
▼ ▼ ▼
┌──────────────────────────────────────────────────────────┐
│ Lockb0x Codex Validator │
│ • Schema validation (Appendix B) │
│ • Anchor-specific verifiers (plug-ins) │
│ • Cross-anchor consistency (ni, hash) │
└──────────────────────────────────────────────────────────┘
│
▼
┌────────────────────┐
│ Lockb0x Attestation│
│ (JWS / on-chain) │
└────────────────────┘
