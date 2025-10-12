# Lockb0x.Anchor.Eth — Ethereum Anchor & Verifier Modules

This module provides the Ethereum anchoring and verification workflow for the Lockb0x Protocol, supporting the GG24 grant deliverables. It enables any dApp, DAO, or developer to anchor content integrity proofs on Ethereum and verify them independently, following the Lockb0x specification and schema.

## Current Status (October 2025)

- **Protocol Compliance:** Smart contract, SDK, CLI, and tests are implemented and align with the Lockb0x Protocol specification. Anchors are indexed by hash and metadata, supporting the protocol's `anchor_ref` field for interoperability.
- **Testnet Deployment:** Contract and SDK are tested and deployed on Ethereum testnet. Mainnet deployment is planned.
- **Verification:** Anchor and verification workflows are covered by Hardhat and Foundry tests. Verification logic matches protocol requirements for hash and metadata comparison.
- **Interoperability:** Ethereum anchors are compatible with the Lockb0x Codex Entry schema and can be used alongside other anchor types (Stellar, GDrive, notary, OpenTimestamps, etc.).
- **Limitations:** Multi-anchor flows and advanced metadata (e.g., cross-chain, certificate integration) are planned. Mainnet deployment and full protocol pipeline integration are pending.
- **Known Issues:** No major issues; platform limitations may affect advanced features. See protocol documentation for schema details.

## Interoperability & Schema Alignment

- Anchors created by this module are compatible with the Lockb0x Protocol Codex Entry schema (`anchor_ref` field) and can be used in multi-anchor workflows (Stellar, GDrive, notary, OpenTimestamps, etc.).
- For full protocol compliance, ensure metadata matches the latest schema in [`spec/v0.0.2-public-draft.md`](../spec/v0.0.2-public-draft.md).
- See protocol documentation for details on anchor types, schema fields, and verification flows.

## Gaps & Next Steps

- Integrate mainnet deployment and advanced metadata support (cross-chain anchors, certificate integration).
- Expand documentation for multi-anchor workflows and protocol pipeline integration.
- Add more end-to-end examples and contributor guidance for Ethereum anchoring in the context of the Lockb0x Protocol.

For questions or contributions, open an issue or submit a pull request.

## Overview

- **Smart Contract:** `contracts/Lockb0xAnchor.sol` (`Lockb0x_Anchor_Eth`) — Stores hashes and metadata for off-chain artifacts. Anchors are immutable and indexed by hash.
- **SDK:** `sdk/lbx-eth-anchor.js` — Node.js/TypeScript SDK for anchoring and verifying artifacts, compatible with Ethers.js and JSON I/O.
- **CLI Example:** `sdk/examples/cli.mjs` — Command-line tool for anchor/verify operations, supporting file hashing and metadata input.
- **Test Coverage:** Hardhat and Foundry tests for contract and SDK, covering anchor, duplicate, and retrieval scenarios.

## Workflow

1. **Anchor**: Submit a hash (sha256/keccak256) and metadata to the `Lockb0x_Anchor_Eth` contract using the SDK or CLI.
2. **Verify**: Retrieve anchor details and compare the stored hash against a recalculated hash from the artifact.
3. **Demo**: Use CLI or web UI to anchor and verify files in real time. Example CLI usage:
   ```sh
   node sdk/examples/cli.mjs anchor <file> [metadata]
   node sdk/examples/cli.mjs get <hash>
   ```

## Directory Structure

```
Lockb0x.Anchor.Eth/
├── contracts/
│   └── Lockb0xAnchor.sol
├── sdk/
│   ├── lbx-eth-anchor.js
│   └── examples/
│       └── cli.mjs
├── hardhat.config.cjs
├── package.json
├── scripts/
│   ├── deploy.js
│   ├── verify.js
│   └── test/
│       └── Lockb0xAnchor.test.js
├── foundry.toml
├── test/
│   └── Lockb0xAnchor.t.sol
└── README.md
```

## Quick Start

1. **Install dependencies:**
   ```sh
   cd Lockb0x.Anchor.Eth
   npm install
   ```
2. **Compile contract:**
   ```sh
   npx hardhat compile --config hardhat.config.cjs
   ```
3. **Run tests:**
   ```sh
   npx hardhat test --config hardhat.config.cjs
   ```
4. **Deploy contract:**
   ```sh
   node scripts/deploy.js
   ```
5. **Anchor a file (CLI):**
   ```sh
   node sdk/examples/cli.mjs anchor <file> [metadata]
   ```
6. **Verify anchor:**
   ```sh
   node sdk/examples/cli.mjs get <hash>
   ```

## License

MIT / Open Specification
