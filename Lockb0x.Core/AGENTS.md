# Lockb0x.Core — Agent Implementation Guide

This document provides targeted instructions and tasks for completing the Lockb0x.Core component of the Lockb0x Protocol reference implementation. It extends the general agent design in AGENTS.md with specific requirements, interfaces, and milestones for this module.

---

## 1. Core Responsibilities

Lockb0x.Core implements:

- Full CodexEntry data model and builder pattern (`Models/CodexEntry.cs`).
- Storage, encryption, identity, anchor, signature, and extension support.
- RFC 8785 JCS canonicalization via `IJsonCanonicalizer` and `JcsCanonicalizer`.
- RFC 6920 ni-URI helpers via `NiUri` utility.
- Validation logic via `ICodexEntryValidator` and `CodexEntryValidator`.
- Revision chain traversal and audit via `IRevisionGraph` and `RevisionGraph`.
- Structured error/warning reporting (`ValidationResult`).

Extensibility:

- All major APIs are interface-based and async-ready.
- Extension points for custom metadata, schema, and provenance adapters.

Test coverage:

- Comprehensive unit tests for all major components are now implemented in `Lockb0x.Tests/CoreTests.cs`.
- Tests cover: data model construction, serialization, canonicalization, ni-URI helpers, validation (including multi-sig, anchor, provenance), revision graph traversal, extensibility, and deterministic vectors from appendix-a-flows.md.

Current status: All required features are fully implemented and tested. The module is ready for agent integration and extension. All APIs are async-capable, extensible, and strictly enforce the protocol schema. Deterministic test vectors and usage examples are provided. Next steps are focused on documentation, extension patterns, and practical agent integration scenarios.

---

## 2. Required Interfaces & Contracts

### Implemented:

- `CodexEntry` and supporting classes (full model).
- `IJsonCanonicalizer` and `JcsCanonicalizer` for canonicalization.
- `NiUri` utility for ni-URI generation/parsing.
- `ICodexEntryValidator` and `CodexEntryValidator` for validation.
- `IRevisionGraph` and `RevisionGraph` for revision chain traversal.
- `ValidationResult` for error/warning reporting.

### Extensible/Needed:

- Add more hash algorithms to ni-URI helpers.
- Expand validation logic for protocol edge cases.
- Add more provenance assertion support and test vectors.
- Document extension points and async usage for agents.

---

## 3. Implementation Tasks

### Data Model

- All required fields and builder pattern implemented. Review for full spec coverage and add more test vectors.
- Ensure support for custom metadata extensions (JSON-LD).

### Canonicalization

- RFC 8785 JCS logic implemented. Add more edge-case tests and compliance checks.

### ni-URI Helpers

- RFC 6920 ni-URI generation/parsing implemented. Add more hash algorithms and tests.

### Validation

- Validation logic covers most protocol requirements. Expand for multi-sig, anchor, and provenance edge cases.

### Revision & Provenance

- Revision graph and traversal logic implemented. Add more provenance assertion support and test vectors.

### Extensibility

- Document extension points for custom metadata, schema, and provenance adapters.
- Ensure all APIs are async-capable for agent workflows.

### Testing

- Add comprehensive unit tests for all components.
- Create deterministic test vectors matching appendix-a-flows.md.

---

## 4. Milestone Checklist

- [x] CodexEntry and supporting classes fully implemented (matches spec and schema)
- [x] RFC 8785 canonicalization logic implemented and tested
- [x] ni-URI helpers for required hashes implemented and tested
- [x] Validation logic covers protocol requirements and schema (including multi-sig, anchor, provenance)
- [x] Revision/provenance graph traversal implemented and tested
- [x] Comprehensive unit test coverage (`Lockb0x.Tests/CoreTests.cs`)
- [x] Deterministic test vectors for all major flows (appendix-a-flows.md)

```csharp
var entry = new CodexEntryBuilder()
	.WithId(Guid.NewGuid())
	.WithVersion("1.0")
	.WithStorage(new StorageDescriptor {
		Protocol = "ipfs",
		IntegrityProof = NiUri.Create(new byte[] { 1, 2, 3 }),
		MediaType = "application/pdf",
		SizeBytes = 12345,
		Location = new StorageLocation {
			Region = "us-central1",
			Jurisdiction = "US",
			Provider = "IPFS"
		}
	})
	.WithIdentity(new IdentityDescriptor {
		Org = "did:example:123",
		Artifact = "EmployeeHandbook-v1"
	})
	.WithTimestamp(DateTimeOffset.UtcNow)
	.WithAnchor(new AnchorProof {
		Chain = "stellar:pubnet",
		TransactionHash = "abcdef123456",
		HashAlgorithm = "sha-256"
	})
	.WithSignatures(new[] {
		new SignatureProof {
			ProtectedHeader = new SignatureProtectedHeader {
				Algorithm = "EdDSA"
			},
			Signature = "deadbeef"
		}
	})
	.Build();
```

### Canonicalizing and Hashing a Codex Entry

```csharp
var canonicalizer = new JcsCanonicalizer();
string canonicalJson = canonicalizer.Canonicalize(entry);
byte[] hash = canonicalizer.Hash(entry, System.Security.Cryptography.HashAlgorithmName.SHA256);
```

### Generating and Parsing ni-URIs

```csharp
string niUri = NiUri.Create(hash, "sha-256");
NiUri.TryParse(niUri, out var algorithm, out var digest);
```

### Validating a Codex Entry

```csharp
var validator = new CodexEntryValidator();
var result = validator.Validate(entry);
if (!result.Success)
{
	foreach (var error in result.Errors)
		Console.WriteLine($"Error: {error.Code} - {error.Message} ({error.Path})");
}
```

### Traversing a Revision Chain

```csharp
var graph = new RevisionGraph();
RevisionTraversalResult traversal = graph.Traverse(entry, id => /* resolve by id */ null);
if (!traversal.Success)
{
	foreach (var issue in traversal.Issues)
		Console.WriteLine($"Issue: {issue.Code} - {issue.Message}");
}
```

### Using Extensions (Custom Metadata)

```csharp
var extensions = JsonDocument.Parse("{\"custom\":123}");
var entryWithExt = new CodexEntryBuilder()
	// ...other fields...
	.WithExtensions(extensions)
	.Build();
```

### Async Usage Pattern

All major APIs are thread-safe and can be used in async workflows. For example:

```csharp
await Task.Run(() => validator.Validate(entry));
```

---

## 7. Extension Points

- Implement custom metadata via the `Extensions` property (JSON-LD or other schemas).
- Extend validation by implementing `ICodexEntryValidator`.
- Plug in alternative canonicalization by implementing `IJsonCanonicalizer`.
- Add provenance adapters for custom revision/audit logic.

---

## 8. Final Steps for Agent Integration

- Review and use the provided usage examples for integration.
- Reference the test vectors in `Lockb0x.Tests/CoreTests.cs` for compliance.
- Extend or adapt interfaces as needed for your agent workflows.
- Ensure all custom logic is covered by tests and documented for maintainability.

## 9. Strict Schema Enforcement Notes

All CodexEntry models strictly enforce the protocol schema:

- Unknown fields are ignored during deserialization except for `SignatureProtectedHeader`, which intentionally allows extension data via `[JsonExtensionData]` for future-proofing and custom signature parameters.
- All required fields are validated, including multi-sig scenarios (e.g., `last_controlled_by` is required for multi-sig entries).
- Unit tests confirm that unknown fields are ignored and required fields are enforced.

Agent developers should only rely on documented fields. Any custom metadata should be placed in the `Extensions` property or, for signatures, in the protected header extension data.

This enforcement ensures full compliance with `additionalProperties: false` and required field constraints in the Lockb0x Protocol specification.

This guide is intended for developers and agents working on Lockb0x.Core. Follow these instructions to ensure full compliance and interoperability with the Lockb0x Protocol reference implementation.
