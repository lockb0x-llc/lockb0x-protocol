# Lockb0x.Verifier

Lockb0x.Verifier implements the verification pipeline for Codex Entries, certificates, and protocol flows. It supports schema validation, canonicalization, integrity, anchor, revision chain, and certificate validation.

## Key Features

- Verification pipeline for protocol compliance
- Certificate validation and delegation
- Revision chain traversal and audit
- Extensible verifier service interface

## Usage

- Use IVerifierService to verify Codex Entries and certificates
- Integrate with CLI, API, and agent workflows

## Implementation Status

- Interfaces and models defined per spec
- Basic signature and certificate verification implemented and tested
- Full pipeline and diagnostics in progress

## Role in Lockb0x Protocol

Provides robust, standards-compliant verification for digital custody and provenance. See `AGENTS.md` for agent integration guidance.
