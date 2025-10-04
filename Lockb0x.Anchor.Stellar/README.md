# Lockb0x.Anchor.Stellar

Lockb0x.Anchor.Stellar implements blockchain anchoring for Codex Entries using the Stellar network. It supports hash anchoring, transaction metadata, and verification helpers for audit and compliance.

## Key Features

- Anchor CodexEntry hashes in Stellar transactions
- Return anchor metadata: chain, transaction hash, timestamp
- Verification helpers for anchor existence and integrity
- Configurable for testnet/mainnet, account credentials, Horizon endpoints

## Usage

- Use IStellarAnchorService to anchor and verify Codex Entries
- Integrate with protocol pipeline for audit and provenance

## Implementation Status

- In-memory/mock Horizon client implemented and tested
- Real Stellar network integration planned

## Role in Lockb0x Protocol

Provides blockchain anchoring for integrity, auditability, and compliance. See `AGENTS.md` for agent integration guidance.
