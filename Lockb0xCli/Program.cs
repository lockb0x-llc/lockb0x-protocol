using System.CommandLine;
using System.CommandLine.Invocation;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using Lockb0x.Core.Canonicalization;
using Lockb0x.Core.Models;
using Lockb0x.Core.Validation;

namespace Lockb0xCli;

public static class Program
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public static async Task<int> Main(string[] args)
    {
        var root = new RootCommand("Lockb0x CLI — generate and validate schema-compliant Codex Entries")
        {
            CreateCreateCommand(),
            CreateValidateCommand()
        };

        return await root.InvokeAsync(args).ConfigureAwait(false);
    }

    private static Command CreateCreateCommand()
    {
        var command = new Command("create", "Generate a schema-compliant Codex Entry document using the latest schema fields (anchor_ref, protected headers, ni-URIs).");

        var idOption = new Option<string>("--id", description: "Entry identifier (UUID v4)") { IsRequired = true };
        var previousOption = new Option<string?>("--previous-id", description: "Optional previous entry identifier (UUID v4)");
        var versionOption = new Option<string>("--version", () => "1.0", "Protocol version string");

        var storageProtocolOption = new Option<string>("--storage-protocol", description: "Storage protocol identifier (ipfs, s3, gdrive, etc.)") { IsRequired = true };
        var storageIntegrityOption = new Option<string>("--storage-integrity", description: "Integrity proof ni-URI (e.g., ni:///sha-256;<digest>)") { IsRequired = true };
        var storageMediaTypeOption = new Option<string>("--storage-media-type", description: "RFC 6838 media type") { IsRequired = true };
        var storageSizeOption = new Option<long>("--storage-size", description: "Stored object size in bytes") { IsRequired = true };
        var storageRegionOption = new Option<string>("--storage-region", description: "Storage region identifier") { IsRequired = true };
        var storageJurisdictionOption = new Option<string>("--storage-jurisdiction", description: "Storage jurisdiction code") { IsRequired = true };
        var storageProviderOption = new Option<string>("--storage-provider", description: "Storage provider name") { IsRequired = true };

        var orgOption = new Option<string>("--org", description: "Primary DID or account controlling the Codex Entry") { IsRequired = true };
        var processOption = new Option<string?>("--process", description: "Optional subordinate DID or process identifier");
        var artifactOption = new Option<string>("--artifact", description: "Artifact or record identifier") { IsRequired = true };
        var subjectOption = new Option<string?>("--subject", description: "Optional DID representing the subject of the entry");

        var timestampOption = new Option<string>("--timestamp", () => DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture), "Timestamp in ISO 8601 format (UTC recommended)");

        var chainOption = new Option<string>("--anchor-chain", description: "Anchor chain or adapter identifier (CAIP-2, gdrive, solid, notary, opentimestamps, rfc3161)") { IsRequired = true };
        var anchorRefOption = new Option<string>("--anchor-ref", description: "Anchor reference (transaction hash, attestation URI, document identifier)") { IsRequired = true };
        var hashAlgOption = new Option<string>("--anchor-hash-alg", () => "SHA256", "Anchor hash algorithm (SHA256 or SHA3-256)");
        var tokenOption = new Option<string?>("--anchor-token-id", description: "Optional token identifier for NFT-based anchors");

        var signatureAlgOption = new Option<string>("--signature-alg", () => "EdDSA", "Signature algorithm identifier (EdDSA, ES256K, RS256)");
        var signatureKidOption = new Option<string>("--signature-kid", description: "Key identifier for the protected header") { IsRequired = true };
        var signatureValueOption = new Option<string>("--signature-value", description: "Base64URL encoded signature value") { IsRequired = true };

        var outputOption = new Option<FileInfo?>("--output", description: "Optional path to write the generated Codex Entry JSON document");

        command.AddOption(idOption);
        command.AddOption(previousOption);
        command.AddOption(versionOption);
        command.AddOption(storageProtocolOption);
        command.AddOption(storageIntegrityOption);
        command.AddOption(storageMediaTypeOption);
        command.AddOption(storageSizeOption);
        command.AddOption(storageRegionOption);
        command.AddOption(storageJurisdictionOption);
        command.AddOption(storageProviderOption);
        command.AddOption(orgOption);
        command.AddOption(processOption);
        command.AddOption(artifactOption);
        command.AddOption(subjectOption);
        command.AddOption(timestampOption);
        command.AddOption(chainOption);
        command.AddOption(anchorRefOption);
        command.AddOption(hashAlgOption);
        command.AddOption(tokenOption);
        command.AddOption(signatureAlgOption);
        command.AddOption(signatureKidOption);
        command.AddOption(signatureValueOption);
        command.AddOption(outputOption);

        command.SetHandler(ctx =>
        {
            var parse = ctx.ParseResult;
            var id = parse.GetValueForOption(idOption)!;
            var previousId = parse.GetValueForOption(previousOption);
            var version = parse.GetValueForOption(versionOption)!;

            var storage = new StorageDescriptor
            {
                Protocol = parse.GetValueForOption(storageProtocolOption)!,
                IntegrityProof = parse.GetValueForOption(storageIntegrityOption)!,
                MediaType = parse.GetValueForOption(storageMediaTypeOption)!,
                SizeBytes = parse.GetValueForOption(storageSizeOption),
                Location = new StorageLocation
                {
                    Region = parse.GetValueForOption(storageRegionOption)!,
                    Jurisdiction = parse.GetValueForOption(storageJurisdictionOption)!,
                    Provider = parse.GetValueForOption(storageProviderOption)!
                }
            };

            var identity = new IdentityDescriptor
            {
                Org = parse.GetValueForOption(orgOption)!,
                Process = parse.GetValueForOption(processOption),
                Artifact = parse.GetValueForOption(artifactOption)!,
                Subject = parse.GetValueForOption(subjectOption)
            };

            if (!DateTimeOffset.TryParse(parse.GetValueForOption(timestampOption), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var timestamp))
            {
                ctx.Console.Error.WriteLine("Invalid timestamp. Use ISO 8601 format (e.g., 2025-10-01T00:00:00Z).");
                ctx.ExitCode = 1;
                return;
            }

            var anchor = new AnchorProof
            {
                Chain = parse.GetValueForOption(chainOption)!,
                Reference = parse.GetValueForOption(anchorRefOption)!,
                HashAlgorithm = parse.GetValueForOption(hashAlgOption)!,
                TokenId = parse.GetValueForOption(tokenOption)
            };

            var signature = new SignatureProof
            {
                Protected = new SignatureProtectedHeader
                {
                    Algorithm = parse.GetValueForOption(signatureAlgOption)!,
                    KeyId = parse.GetValueForOption(signatureKidOption)!
                },
                Signature = parse.GetValueForOption(signatureValueOption)!
            };

            var builder = new CodexEntryBuilder()
                .WithId(id)
                .WithVersion(version)
                .WithStorage(storage)
                .WithIdentity(identity)
                .WithTimestamp(timestamp)
                .WithAnchor(anchor)
                .WithSignatures(new[] { signature });

            if (!string.IsNullOrWhiteSpace(previousId))
            {
                builder.WithPreviousId(previousId);
            }

            var entry = builder.Build();
            var validator = new CodexEntryValidator();
            var validation = validator.Validate(entry);
            if (!validation.Success)
            {
                ctx.Console.Error.WriteLine("Codex Entry failed validation:");
                foreach (var error in validation.Errors)
                {
                    ctx.Console.Error.WriteLine($" - [{error.Code}] {error.Message} ({error.Path})");
                }

                ctx.ExitCode = 1;
                return;
            }

            var json = JsonSerializer.Serialize(entry, SerializerOptions);
            var output = parse.GetValueForOption(outputOption);
            if (output is not null)
            {
                File.WriteAllText(output.FullName, json);
                ctx.Console.Out.WriteLine($"Codex Entry written to {output.FullName}");
            }
            else
            {
                ctx.Console.Out.WriteLine(json);
            }

            ctx.ExitCode = 0;
        });

        return command;
    }

    private static Command CreateValidateCommand()
    {
        var command = new Command("validate", "Validate a Codex Entry JSON document against the latest schema fields (anchor_ref, protected headers, ni-URI integrity).");
        var fileOption = new Option<FileInfo>("--entry", description: "Path to the Codex Entry JSON document") { IsRequired = true };
        var networkOption = new Option<string?>("--anchor-network", description: "Optional expected anchor network (e.g., stellar:pubnet)");

        command.AddOption(fileOption);
        command.AddOption(networkOption);

        command.SetHandler(ctx =>
        {
            var parse = ctx.ParseResult;
            var file = parse.GetValueForOption(fileOption)!;
            if (!file.Exists)
            {
                ctx.Console.Error.WriteLine($"Entry file '{file.FullName}' does not exist.");
                ctx.ExitCode = 1;
                return;
            }

            var json = File.ReadAllText(file.FullName);
            CodexEntry? entry;
            try
            {
                entry = JsonSerializer.Deserialize<CodexEntry>(json, SerializerOptions);
            }
            catch (Exception ex)
            {
                ctx.Console.Error.WriteLine($"Failed to parse Codex Entry: {ex.Message}");
                ctx.ExitCode = 1;
                return;
            }

            if (entry is null)
            {
                ctx.Console.Error.WriteLine("Entry file did not contain a Codex Entry payload.");
                ctx.ExitCode = 1;
                return;
            }

            var validator = new CodexEntryValidator();
            var context = parse.GetValueForOption(networkOption) is { Length: > 0 } network
                ? new CodexEntryValidationContext(network)
                : null;
            var validation = validator.Validate(entry, context);

            if (!validation.Success)
            {
                ctx.Console.Error.WriteLine("Codex Entry validation failed:");
                foreach (var error in validation.Errors)
                {
                    ctx.Console.Error.WriteLine($" - [{error.Code}] {error.Message} ({error.Path})");
                }

                if (validation.Warnings.Count > 0)
                {
                    ctx.Console.Error.WriteLine("Warnings:");
                    foreach (var warning in validation.Warnings)
                    {
                        ctx.Console.Error.WriteLine($" - [{warning.Code}] {warning.Message} ({warning.Path})");
                    }
                }

                ctx.ExitCode = 1;
                return;
            }

            var canonicalizer = new JcsCanonicalizer();
            var canonicalJson = canonicalizer.Canonicalize(entry);
            var hash = canonicalizer.Hash(entry, HashAlgorithmName.SHA256);

            ctx.Console.Out.WriteLine("Codex Entry is valid.");
            ctx.Console.Out.WriteLine($"Canonical hash (base64url): {ToBase64Url(hash)}");
            ctx.Console.Out.WriteLine("Canonical JSON:");
            ctx.Console.Out.WriteLine(canonicalJson);
            ctx.ExitCode = 0;
        });

        return command;
    }

    private static string ToBase64Url(ReadOnlySpan<byte> bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-', StringComparison.Ordinal)
            .Replace('/', '_', StringComparison.Ordinal);
    }
}
