# Lockb0x.Signing — Agent Implementation Guide

This document provides comprehensive guidance for AI Agents and developers integrating with the Lockb0x.Signing module of the Lockb0x Protocol reference implementation. It covers responsibilities, integration patterns, extension points, and best practices for secure, interoperable signing workflows.

---

## 1. Overview

Lockb0x.Signing implements cryptographic signing and verification for Codex Entries, supporting multi-signature policies and key management. It is designed for interoperability with other Lockb0x modules and external key stores.

---

## 2. Responsibilities

- Sign canonicalized Codex Entries using JOSE JWS and COSE Sign1 formats.
- Support EdDSA (Ed25519), ES256K (secp256k1), and RS256 algorithms.
- Enforce multi-sig policies and key revocation.
- Verify signatures and attach signature proofs to entries.
- Integrate with external key stores (HSM, DID, Stellar, etc.).

---

## 3. Integration Patterns

### a. Signing Workflow

1. **Canonicalize Entry**
   Use Lockb0x.Core to canonicalize the CodexEntry before signing.
2. **Sign Payload**
   Use ISigningService to sign the canonicalized payload with the selected key and algorithm.
3. **Attach Signature**
   Add the resulting SignatureProof to the CodexEntry.
4. **Multi-Sig Enforcement**
   Ensure required threshold and key policies are met.
5. **Verification**
   Use ISigningService to verify signatures during validation.

### b. Key Management

- List available signing keys.
- Revoke or rotate keys as needed.
- Integrate with external key management systems.

---

## 4. API Contracts

- `Sign(payload: byte[], key: SigningKey, alg: string): SignatureProof`
- `Verify(payload: byte[], signature: SignatureProof): bool`
- `ListKeys(): SigningKey[]`
- `RevokeKey(keyId: string): void`

---

## 5. Error Handling

- Signature failures return algorithm/key-specific error codes.
- Revoked or expired keys trigger explicit validation errors.
- Multi-sig policy violations are surfaced as threshold errors.

---

## 6. Extensibility

- Add support for new algorithms via strategy pattern.
- Integrate external key stores (HSM, DID, Stellar, etc.).
- Extend signature formats and verification logic via interfaces.

---

## 7. Example Workflow

```csharp
// Canonicalize entry using Lockb0x.Core
string canonicalJson = JcsCanonicalizer.Canonicalize(entry);

// Sign the canonicalized payload
var signature = signingService.Sign(
    Encoding.UTF8.GetBytes(canonicalJson),
    mySigningKey,
    "EdDSA"
);

// Attach signature to entry
entry.Signatures.Add(signature);

// Verify signature
bool isValid = signingService.Verify(
    Encoding.UTF8.GetBytes(canonicalJson),
    signature
);
```

---

## 8. Best Practices for AI Agents

- Always canonicalize entries before signing.
- Validate entries and signatures before anchoring or sharing.
- Enforce multi-sig policies for sensitive workflows.
- Use async APIs for scalable agent operations.
- Document custom key management and signature extensions.

---

## 9. Compliance & Interoperability

- All signatures must comply with the protocol specification and supported algorithms.
- Extension points are documented for agent interoperability.
- Use provided test vectors to ensure agent compliance.

---

For further details, see the protocol specification and module-level AGENTS.md files in Lockb0x.Core and related projects.
