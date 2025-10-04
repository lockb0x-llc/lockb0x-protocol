# Lockb0x.Storage

Lockb0x.Storage provides storage adapters for the Lockb0x Protocol, enabling secure, verifiable, and portable data storage. The reference implementation includes an IPFS adapter supporting RFC 6920 ni-URI integrity proofs and CID validation.

## Key Features

- IPFS storage adapter (StoreAsync, FetchAsync)
- RFC 6920 ni-URI integrity proof and CID validation
- Metadata for audit, certificate, and protocol compliance
- Async agent workflow support

## Usage

- Store files with IpfsStorageAdapter
- Retrieve files by CID for verification and audit
- Integrate with CodexEntry builder and protocol validation

## Implementation Status

- IPFS adapter fully implemented and tested
- S3, Filecoin, and other backends planned

## Role in Lockb0x Protocol

Provides portable, verifiable storage for Codex Entries and associated data. See `AGENTS.md` for agent integration guidance.
