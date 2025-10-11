# Lockb0x.Api

Lockb0x.Api exposes the Lockb0x protocol as a web API for agentic, automated, and interactive workflows. It provides endpoints for Codex Entry creation, signing, anchoring, verification, and certificate management.

## Key Features

- `POST /api/codex/create` validates a Codex Entry payload (with `anchor_ref`, protected signature headers, ni-URI storage descriptors) and returns canonical JSON plus a base64url SHA-256 hash.
- `POST /api/codex/validate` returns structured validation results (errors + warnings) with optional anchor network hints.
- `GET /api/codex/template` supplies a schema-aligned template entry for agent onboarding.
- Swagger/OpenAPI metadata enabled in development for quick contract discovery.

## Usage

### Validate and Canonicalize an Entry

```bash
curl -X POST http://localhost:5000/api/codex/create \
  -H "Content-Type: application/json" \
  -d @entry.json
```

### Validation Only

```bash
curl -X POST "http://localhost:5000/api/codex/validate?anchor_network=stellar:testnet" \
  -H "Content-Type: application/json" \
  -d @entry.json
```

## Implementation Status

- Schema-aligned Codex endpoints implemented.
- Signing, anchoring, and certification endpoints will layer on shared services in future milestones.

## Role in Lockb0x Protocol

Enables agentic and automated workflows for digital custody, provenance, and compliance. See AGENTS.md for integration guidance.
