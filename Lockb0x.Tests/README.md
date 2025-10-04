# Lockb0x.Tests

Lockb0x.Tests contains deterministic unit and integration tests for the Lockb0x protocol reference implementation. It ensures protocol compliance, robustness, and extensibility across all modules.

## Key Features

- Full coverage for Core, Signing, Storage, Anchor, Certificates, Verifier
- Deterministic vectors and protocol pipeline tests
- Mock adapters for CI and agentic workflows
- Platform-specific test handling

## Usage

- Run tests with `dotnet test` for protocol compliance
- Use as reference for agentic and contributor test patterns

## Implementation Status

- All major flows and edge cases covered
- Platform-specific gaps documented in AGENTS.md

## Role in Lockb0x Protocol

Ensures protocol robustness and compliance. See AGENTS.md for agentic test guidance.
