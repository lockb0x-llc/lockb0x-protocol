# Lockb0x.Tests — AGENTS.md

## Agentic Test Guide

Lockb0x.Tests provides deterministic unit and integration tests for protocol compliance and robustness. This guide helps agent developers understand test patterns, coverage, and platform-specific considerations.

### Key Test Patterns

- Full protocol pipeline tests for Core, Signing, Storage, Anchor, Certificates, Verifier
- Deterministic vectors for reproducible agentic automation
- Mock adapters for CI and agentic workflows
- Platform-specific test handling (e.g., X.509 on macOS/Linux)

### Best Practices

- Use reference tests as templates for agentic and contributor tests
- Document platform-specific gaps and workarounds
- Validate agent workflows against protocol tests

### Next Steps

- Expand test coverage for new features and edge cases
- Integrate with CLI and API for automated agentic testing

For more details, see the main README.md and protocol documentation.
