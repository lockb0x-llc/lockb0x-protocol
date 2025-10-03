# Lockb0x.Core

Lockb0x.Core is the foundational module of the Lockb0x Protocol reference implementation. It provides the canonical data model, schema validation, canonicalization, revision chain management, and core utilities required for building, validating, and auditing Codex Entries.

## Key Features

- **CodexEntry Data Model**: Defines the portable, signed JSON structure for digital custody, integrity, and provenance.
- **Builder Pattern**: Enables flexible and type-safe construction of Codex Entries.
- **RFC 8785 JCS Canonicalization**: Ensures deterministic JSON serialization for signing and verification.
- **RFC 6920 ni-URI Helpers**: Generates and parses integrity URIs for storage proofs.
- **Validation**: Enforces protocol schema, required fields, and multi-sig/anchor/provenance rules.
- **Revision Chain Management**: Supports audit trails and provenance traversal.
- **Extensibility**: All major APIs are interface-based and async-ready, with extension points for custom metadata, schema, and provenance adapters.

## Usage

Lockb0x.Core is intended for use by agents, services, and applications that need to:

- Construct and validate Codex Entries
- Canonicalize entries for signing and anchoring
- Traverse revision chains for audit and compliance
- Integrate with storage, signing, anchoring, and certificate modules

## Example Workflow

1. **Build a Codex Entry** using the builder pattern and required metadata.
2. **Canonicalize the entry** for signing and anchoring.
3. **Validate the entry** against the protocol schema.
4. **Traverse revision chains** for audit and provenance.

See `AGENTS.md` for detailed usage examples and integration guidance.

## Implementation Status

- Fully implemented and tested.
- All major features and APIs are covered by deterministic unit tests.
- Strict schema enforcement and extension points documented.

## Role in Lockb0x Protocol

Lockb0x.Core is the backbone of the Lockb0x Protocol, enabling portable, verifiable, and extensible digital custody for any data, on any backend or blockchain. It is designed for interoperability, compliance, and agent-driven workflows.

For more information, see the main [`README.md`](../README.md) and protocol documentation.
