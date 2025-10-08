# Lockb0x.Anchor.Eth — Ethereum Anchor & Verifier Modules

This module provides the Ethereum anchoring and verification workflow for the Lockb0x Protocol, supporting the GG24 grant deliverables. It enables any dApp, DAO, or developer to anchor content integrity proofs on Ethereum and verify them independently, following the Lockb0x specification.

This module implements the Ethereum anchoring workflow for the Lockb0x Protocol, as described in the GG24 grant submission.

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
├── hardhat.config.js
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
   npx hardhat compile
   ```
3. **Run tests:**
   ```sh
   npx hardhat test
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

## Grant Reference

See [GG24 Grant Submission Plan](../grant-management/GG24/GRANT-SUBMISSION-PLAN.md) and [AGENTS.md](../grant-management/GG24/AGENTS.md) for context, roadmap, and deliverables. This module fulfills the Ethereum anchor and verifier requirements for the GG24 Developer Tooling & Infrastructure grant.

## License

MIT / Open Specification
