# Lockb0x.Storage — AGENTS.md

## Implementation Status (October 2025)

- **IpfsStorageAdapter** is fully implemented and integrated with the Lockb0x Protocol reference solution.
- All required metadata (protocol, integrity proof, CID, location, size, media type) is returned and validated.
- RFC 6920 ni-URI integrity proof and CID validation are enforced.
- Unit tests cover all major flows: file storage, retrieval, CID/hash validation, and error handling.
- Adapter is compatible with Codex Entry builder and protocol validation.

## Usage Guidance

### Storing Files

Use `IpfsStorageAdapter.StoreAsync` to store files in IPFS. The returned `StorageDescriptor` can be used to construct a Codex Entry for signing and anchoring.

### Retrieving Files

Use `IpfsStorageAdapter.FetchAsync` to retrieve files by CID for verification and audit. Streaming and partial reads are supported.

### Integration

- The adapter is designed for async agent workflows and integrates with the protocol's end-to-end flow: file → IPFS → Codex Entry → sign → anchor → verify.
- All metadata is available for audit and certificate generation.

### Testing

- Deterministic unit tests use mock HTTP clients for CI.
- See `Lockb0x.Tests/StorageAdapterTests.cs` for reference coverage.

## Gaps & Next Steps

- Only IPFS is implemented; S3, Filecoin, and other backends are not yet supported.
- No step-by-step contributor guide for setting up IPFS nodes or running integration tests.
- Error handling is present, but centralized logging and diagnostics are not yet implemented.
- CLI integration and end-to-end flows are pending.

## Contributor Notes

- Always canonicalize and hash file content before storing.
- Use secure, reliable IPFS nodes or pinning services.
- Log all storage operations and errors for auditability.
- Follow .NET async/await and dependency injection patterns.

## References

- [Storage Adapters Spec](../../spec/storage-adapters.md)
- [Appendix A Flows](../../spec/appendix-a-flows.md)
- [Data Model](../../spec/data-model.md)
- [Reference Implementation](../../spec/reference-implementation.md)
- [RFC 6920: ni-URI](https://datatracker.ietf.org/doc/html/rfc6920)
- [IPFS Documentation](https://docs.ipfs.tech/)

---

For questions or contributions, open an issue or submit a pull request.
