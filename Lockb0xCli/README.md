# Lockb0xCli

Lockb0xCli is the command-line interface for the Lockb0x protocol reference implementation. It enables agentic, automated, and interactive workflows for protocol operations.

## Key Features

- `create` command generates schema-compliant Codex Entries using the latest fields (`anchor_ref`, protected signature headers, ni-URI storage descriptors).
- `validate` command verifies an existing Codex Entry JSON document, reports structured validation diagnostics, and returns canonical JSON + hash.
- Built-in canonicalization and SHA-256 hashing using `Lockb0x.Core` helpers for deterministic automation.
- Snake-case JSON output aligned with the public schema.

## Usage

### Generate a Codex Entry

```bash
lockb0x create \
  --id 2d5f4b5e-1bfa-43ea-9d0a-6d6b79b3a9b2 \
  --version 1.0 \
  --storage-protocol ipfs \
  --storage-integrity "ni:///sha-256;abcdef" \
  --storage-media-type application/json \
  --storage-size 1024 \
  --storage-region us-east-1 \
  --storage-jurisdiction US \
  --storage-provider ipfs \
  --org did:example:issuer \
  --artifact example-artifact \
  --anchor-chain stellar:testnet \
  --anchor-ref 0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef \
  --signature-kid did:example:issuer#ed25519 \
  --signature-value base64urlsignature
```

### Validate an Existing Entry

```bash
lockb0x validate --entry entry.json --anchor-network stellar:testnet
```

The validator reports errors, warnings, canonical JSON, and a base64url hash for reproducible workflows.

## Implementation Status

- Schema-aligned create/validate commands implemented.
- Multi-step commands (sign, anchor, certify, verify) will be layered atop shared services in future milestones.

## Role in Lockb0x Protocol

Enables agentic and automated workflows for digital custody, provenance, and compliance. See AGENTS.md for integration guidance.
