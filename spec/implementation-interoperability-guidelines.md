16. Implementation Interoperability Guidelines (Normative)

This section defines the interoperability requirements for any Lockb0x‑compliant
implementation. It builds upon the core data model and signing semantics
defined earlier in this specification and ensures that implementations from
different providers can interoperate without prior coordination.

16.1 Anchor and Storage Interface Compliance 1. Each implementation MUST implement the abstract interfaces defined in
Section 3.2 (ILockb0xAnchorProvider and ILockb0xStorageProvider). 2. Each provider MUST produce verifiable digital signatures that bind the
payload integrity hash (ni) to both an identity record and a storage
descriptor. 3. Each anchor and storage descriptor MUST serialize deterministically using
the RFC 8785 JSON Canonicalization Scheme (JCS). 4. Signatures MUST be encoded as JWS, COSE_Sign1, or an equivalent detached
digital signature format, and MUST specify an algorithm identifier
recognized in Section 6. 5. All descriptors MUST contain a type and provider field that uniquely
identify the ecosystem adapter.

16.2 Cross‑Anchor Consistency 1. All signatures within a Codex MUST reference the same ni value and
integrity hash. 2. When multiple anchor sets are present (see Appendix D), each anchor set
MUST be independently verifiable while preserving cross‑anchor consistency
of:
• ni (the canonical integrity reference),
• the declared signature algorithm, and
• any cover signature (Section 5.6).

16.3 Cover and Attestation Semantics 1. A cover signature MAY be included to cryptographically confirm user
consent or provenance across anchors. The cover input MUST be derived as
the hash of a canonical object containing ni and the hash of each anchor’s
signature. Supported formats include WebAuthn assertions, EIP‑4361/EIP‑712
wallet signatures, or equivalent user‑held credentials. 2. A Lockb0x Attestation issued by a validator service MUST be
represented as a signed JWS object containing:
• the Codex ni,
• the timestamp of validation, and
• a digest of all validation results.

16.4 Verification Behavior

Implementations that perform verification—whether as standalone validators, as
part of a wallet, or within a custodial gateway—MUST: 1. Validate each digital signature against the appropriate public key source
(e.g., OIDC JWKS, blockchain state, or trusted key registry). 2. Confirm that canonicalization and digest calculations yield the expected ni. 3. Enforce schema validity according to Appendix B. 4. Record verification outcomes in a verifiable log or attestation record.

16.5 Extensibility and Provider Descriptors 1. New providers SHOULD publish a self‑describing provider descriptor
conforming to the schema in schema/provider-descriptor.schema.json. The
descriptor SHOULD include:
• a unique providerId,
• a stable providerUri,
• the identity and storage schemes in use,
• supported signature algorithms, and
• maintainer contact information. 2. Providers MUST NOT alter the normative schema or canonicalization
semantics when defining new ecosystems. New providers MAY introduce
additional optional fields provided they do not conflict with existing
semantics.

These guidelines ensure that any new implementation remains interoperable with
the core Lockb0x Protocol while allowing innovation across diverse identity and
storage ecosystems.
