# Lockb0x.Signing

Lockb0x.Signing provides cryptographic signing and verification for Codex Entries in the Lockb0x Protocol. It supports Ed25519, ES256K, and RS256 algorithms, multi-signature policies, and key management. The module is designed for interoperability with other Lockb0x modules and external key stores.

## Key Features

- JOSE JWS and COSE Sign1 support
- EdDSA (Ed25519), ES256K (secp256k1), RS256 algorithms
- Multi-signature enforcement and key revocation
- Extensible signing service interface
- Integration points for HSM, DID, Stellar, and external key stores

## Usage

- Canonicalize CodexEntry with Lockb0x.Core
- Sign payload using ISigningService
- Attach signature proof to CodexEntry
- Verify signatures and enforce multi-sig policies

## Implementation Status

- Fully implemented and tested for Ed25519, ES256K, RS256
- Multi-sig, key revocation, and error handling covered
- External key store integration planned

## Role in Lockb0x Protocol

Enables secure, portable, and interoperable digital custody and provenance for any data. See `AGENTS.md` for agent integration guidance.
