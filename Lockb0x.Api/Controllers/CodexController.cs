using System.Security.Cryptography;
using System.Text.Json.Serialization;
using Lockb0x.Core.Canonicalization;
using Lockb0x.Core.Models;
using Lockb0x.Core.Validation;
using Microsoft.AspNetCore.Mvc;

namespace Lockb0x.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CodexController : ControllerBase
{
    private readonly ICodexEntryValidator _validator;
    private readonly IJsonCanonicalizer _canonicalizer;

    public CodexController(ICodexEntryValidator validator, IJsonCanonicalizer canonicalizer)
    {
        _validator = validator;
        _canonicalizer = canonicalizer;
    }

    /// <summary>
    /// Validate a Codex Entry document and return canonical representation details.
    /// </summary>
    [HttpPost("create")]
    [ProducesResponseType(typeof(CodexEntryEnvelope), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public ActionResult<CodexEntryEnvelope> Create([FromBody] CodexEntry entry)
    {
        var validation = _validator.Validate(entry);
        if (!validation.Success)
        {
            return BadRequest(ValidationSummary.From(validation));
        }

        var canonicalJson = _canonicalizer.Canonicalize(entry);
        var hash = _canonicalizer.Hash(entry, HashAlgorithmName.SHA256);
        var envelope = new CodexEntryEnvelope(entry, canonicalJson, ToBase64Url(hash));
        return Ok(envelope);
    }

    /// <summary>
    /// Validate a Codex Entry document and return validation diagnostics.
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ValidationSummary), StatusCodes.Status200OK)]
    public ActionResult<ValidationSummary> Validate([FromBody] CodexEntry entry, [FromQuery(Name = "anchor_network")] string? anchorNetwork)
    {
        var context = string.IsNullOrWhiteSpace(anchorNetwork) ? null : new CodexEntryValidationContext(anchorNetwork);
        var result = _validator.Validate(entry, context);
        return Ok(ValidationSummary.From(result));
    }

    /// <summary>
    /// Provide a schema-aligned template illustrating the latest fields (anchor_ref, protected headers, ni-URI storage descriptors).
    /// </summary>
    [HttpGet("template")]
    [ProducesResponseType(typeof(CodexEntryEnvelope), StatusCodes.Status200OK)]
    public ActionResult<CodexEntryEnvelope> Template()
    {
        var now = DateTimeOffset.UtcNow;
        var entry = new CodexEntryBuilder()
            .WithId(Guid.NewGuid())
            .WithVersion("1.0")
            .WithStorage(new StorageDescriptor
            {
                Protocol = "ipfs",
                IntegrityProof = "ni:///sha-256;example",
                MediaType = "application/json",
                SizeBytes = 1024,
                Location = new StorageLocation
                {
                    Region = "us-east-1",
                    Jurisdiction = "US",
                    Provider = "ipfs"
                }
            })
            .WithIdentity(new IdentityDescriptor
            {
                Org = "did:example:issuer",
                Artifact = "example-artifact"
            })
            .WithTimestamp(now)
            .WithAnchor(new AnchorProof
            {
                Chain = "stellar:testnet",
                Reference = "0000000000000000000000000000000000000000000000000000000000000000",
                HashAlgorithm = "SHA256"
            })
            .WithSignatures(new[]
            {
                new SignatureProof
                {
                    Protected = new SignatureProtectedHeader
                    {
                        Algorithm = "EdDSA",
                        KeyId = "did:example:issuer#ed25519"
                    },
                    Signature = "base64url-signature"
                }
            })
            .Build();

        var canonicalJson = _canonicalizer.Canonicalize(entry);
        var hash = _canonicalizer.Hash(entry, HashAlgorithmName.SHA256);
        return Ok(new CodexEntryEnvelope(entry, canonicalJson, ToBase64Url(hash)));
    }

    private static string ToBase64Url(ReadOnlySpan<byte> value)
    {
        return Convert.ToBase64String(value)
            .TrimEnd('=')
            .Replace('+', '-', StringComparison.Ordinal)
            .Replace('/', '_', StringComparison.Ordinal);
    }
}

public sealed record CodexEntryEnvelope(
    [property: JsonPropertyName("entry")] CodexEntry Entry,
    [property: JsonPropertyName("canonical_json")] string CanonicalJson,
    [property: JsonPropertyName("canonical_hash")] string CanonicalHashBase64Url);

public sealed record ValidationSummary(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("errors")] IReadOnlyList<CodexEntryValidationError> Errors,
    [property: JsonPropertyName("warnings")] IReadOnlyList<CodexEntryValidationWarning> Warnings)
{
    public static ValidationSummary From(ValidationResult result)
        => new(result.Success, result.Errors, result.Warnings);
}
