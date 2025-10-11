using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lockb0x.Anchor.Stellar;
using Lockb0x.Certificates;
using Lockb0x.Certificates.Models;
using Lockb0x.Core.Canonicalization;
using Lockb0x.Core.Models;
using Lockb0x.Core.Revision;
using Lockb0x.Core.Utilities;
using Lockb0x.Core.Validation;
using Lockb0x.Signing;
using Lockb0x.Storage;

namespace Lockb0x.Verifier;

/// <summary>
/// Default implementation of the verifier pipeline orchestrating validation across Lockb0x modules.
/// </summary>
public sealed class VerifierService : IVerifierService
{
    private readonly ICodexEntryValidator _validator;
    private readonly IJsonCanonicalizer _canonicalizer;
    private readonly ISigningService _signingService;
    private readonly IReadOnlyDictionary<string, IStorageAdapter> _storageAdapters;
    private readonly IStellarAnchorService? _anchorService;
    private readonly ICertificateService? _certificateService;
    private readonly IRevisionGraph _revisionGraph;
    private readonly Func<string, CodexEntry?>? _revisionResolver;
    private readonly StorageLocationResolver? _storageLocationResolver;
    private readonly Func<AnchorProof, string?>? _anchorNetworkResolver;
    private readonly string? _defaultAnchorNetwork;
    private readonly int? _maxRevisionDepth;

    public VerifierService(VerifierDependencies dependencies)
    {
        ArgumentNullException.ThrowIfNull(dependencies);

        _validator = dependencies.Validator ?? throw new ArgumentNullException(nameof(dependencies.Validator));
        _canonicalizer = dependencies.Canonicalizer ?? throw new ArgumentNullException(nameof(dependencies.Canonicalizer));
        _signingService = dependencies.SigningService ?? throw new ArgumentNullException(nameof(dependencies.SigningService));
        _storageAdapters = dependencies.StorageAdapters is null
            ? new Dictionary<string, IStorageAdapter>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, IStorageAdapter>(dependencies.StorageAdapters, StringComparer.OrdinalIgnoreCase);
        _anchorService = dependencies.AnchorService;
        _certificateService = dependencies.CertificateService;
        _revisionGraph = dependencies.RevisionGraph ?? new RevisionGraph();
        _revisionResolver = dependencies.RevisionResolver;
        _storageLocationResolver = dependencies.StorageLocationResolver;
        _anchorNetworkResolver = dependencies.AnchorNetworkResolver;
        _defaultAnchorNetwork = dependencies.DefaultAnchorNetwork;
        _maxRevisionDepth = dependencies.MaxRevisionDepth;
    }

    public async Task<VerificationResult> VerifyAsync(CodexEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var result = new VerificationResult();
        var validSignatureKeyIds = new HashSet<string>(StringComparer.Ordinal);
        byte[]? canonicalPayload = null;
        byte[]? anchorHash = null;
        string? anchorHashAlgorithm = null;
        string? integrityAlgorithm = null;
        byte[]? integrityDigest = null;

        await RunStepAsync("Schema validation", result, async context =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var validation = _validator.Validate(entry, new CodexEntryValidationContext(entry.Anchor?.Chain));
            if (!validation.Success)
            {
                foreach (var error in validation.Errors)
                {
                    context.AddError(error.Code, error.Message, error.Path);
                }
            }

            foreach (var warning in validation.Warnings)
            {
                context.AddWarning(warning.Code, warning.Message, warning.Path);
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);

        await RunStepAsync("Canonicalization", result, async context =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var snapshot = CodexEntryCanonicalPayload.CreateSnapshot(entry);
                var canonicalJson = _canonicalizer.Canonicalize(snapshot);
                canonicalPayload = Encoding.UTF8.GetBytes(canonicalJson);
                context.AddMetadata("payload_length", canonicalPayload.Length.ToString(CultureInfo.InvariantCulture));

                if (!string.IsNullOrWhiteSpace(entry.Anchor?.HashAlgorithm) && TryResolveHashAlgorithm(entry.Anchor.HashAlgorithm, out var hashAlgorithm))
                {
                    anchorHash = _canonicalizer.Hash(snapshot, hashAlgorithm);
                    anchorHashAlgorithm = hashAlgorithm.Name;
                    context.AddMetadata("anchor_hash_alg", anchorHashAlgorithm);
                    context.AddMetadata("anchor_hash", Convert.ToHexString(anchorHash).ToLowerInvariant());
                }
                else if (entry.Anchor is not null)
                {
                    context.AddWarning("verifier.canonicalization.unsupported_anchor_hash", $"Unsupported anchor hash algorithm '{entry.Anchor.HashAlgorithm}'.", "anchor.hash_alg");
                }
            }
            catch (Exception ex)
            {
                context.AddError("verifier.canonicalization.failed", $"Failed to canonicalize Codex Entry: {ex.Message}");
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);

        await RunStepAsync("Signature validation", result, async context =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (canonicalPayload is null)
            {
                context.Skip("verifier.signatures.skipped", "Canonical payload unavailable; signature validation skipped.");
                return;
            }

            if (entry.Signatures is null || entry.Signatures.Count == 0)
            {
                context.AddError("verifier.signatures.missing", "No signature proofs were supplied on the Codex Entry.", "signatures");
                return;
            }

            foreach (var signature in entry.Signatures)
            {
                if (signature?.Protected is null)
                {
                    context.AddError("verifier.signatures.invalid", "Signature proof is missing required header metadata.");
                    continue;
                }

                try
                {
                    if (await _signingService.VerifyAsync(canonicalPayload, signature).ConfigureAwait(false))
                    {
                        if (!string.IsNullOrWhiteSpace(signature.Protected.KeyId))
                        {
                            validSignatureKeyIds.Add(signature.Protected.KeyId);
                        }
                    }
                    else
                    {
                        context.AddError("verifier.signatures.invalid", $"Signature with kid '{signature.Protected.KeyId}' failed verification.", "signatures");
                    }
                }
                catch (Exception ex)
                {
                    context.AddError("verifier.signatures.exception", $"Signature verification threw an exception: {ex.Message}");
                }
            }

            if (validSignatureKeyIds.Count == 0)
            {
                context.AddError("verifier.signatures.none_valid", "No valid signatures were found on the Codex Entry.");
            }
            else
            {
                context.AddMetadata("valid_signatures", validSignatureKeyIds.Count.ToString(CultureInfo.InvariantCulture));
            }

            var required = DetermineRequiredSignatures(entry);
            if (validSignatureKeyIds.Count < required)
            {
                context.AddError("verifier.signatures.threshold", $"Valid signature count {validSignatureKeyIds.Count} does not satisfy required threshold {required}.");
            }
        }, cancellationToken).ConfigureAwait(false);

        await RunStepAsync("Integrity proof validation", result, async context =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (entry.Storage is null)
            {
                context.AddError("verifier.integrity.storage_missing", "Storage descriptor is required for integrity verification.", "storage");
                return;
            }

            if (!NiUri.TryParse(entry.Storage.IntegrityProof, out var algorithm, out var digest))
            {
                context.AddError("verifier.integrity.invalid_ni", "storage.integrity_proof is not a valid RFC 6920 ni-URI.", "storage.integrity_proof");
                return;
            }

            integrityAlgorithm = algorithm;
            integrityDigest = digest;
            context.AddMetadata("integrity_algorithm", algorithm);
            context.AddMetadata("integrity_digest", Convert.ToHexString(digest).ToLowerInvariant());

            await Task.CompletedTask.ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);

        await RunStepAsync("Storage verification", result, async context =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (entry.Storage is null)
            {
                context.AddError("verifier.storage.missing_descriptor", "Storage descriptor is required for verification.", "storage");
                return;
            }

            if (!_storageAdapters.TryGetValue(entry.Storage.Protocol, out var adapter))
            {
                context.AddError("verifier.storage.adapter_missing", $"No storage adapter registered for protocol '{entry.Storage.Protocol}'.", "storage.protocol");
                return;
            }

            if (_storageLocationResolver is null)
            {
                context.AddError("verifier.storage.location_resolver_missing", "Storage location resolver has not been configured.");
                return;
            }

            var location = await _storageLocationResolver(entry, cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(location))
            {
                context.AddError("verifier.storage.location_missing", "Storage location resolver did not provide a resource identifier.");
                return;
            }

            context.AddMetadata("storage_location", location);

            try
            {
                if (!await adapter.ExistsAsync(location, cancellationToken).ConfigureAwait(false))
                {
                    context.AddError("verifier.storage.not_found", "Declared storage resource could not be located at the provider.");
                    return;
                }

                var metadata = await adapter.GetMetadataAsync(location, cancellationToken).ConfigureAwait(false);
                if (!string.Equals(metadata.Descriptor.IntegrityProof, entry.Storage.IntegrityProof, StringComparison.Ordinal))
                {
                    context.AddError("verifier.storage.integrity_mismatch", "Stored integrity proof does not match Codex Entry.");
                }

                if (!string.Equals(metadata.Descriptor.MediaType, entry.Storage.MediaType, StringComparison.OrdinalIgnoreCase))
                {
                    context.AddError("verifier.storage.media_type_mismatch", "Stored media type differs from Codex Entry declaration.");
                }

                if (metadata.Descriptor.SizeBytes != entry.Storage.SizeBytes)
                {
                    context.AddError("verifier.storage.size_mismatch", "Stored size does not match Codex Entry declaration.");
                }

                if (integrityDigest is not null && integrityAlgorithm is not null)
                {
                    if (!NiUri.TryParse(metadata.Descriptor.IntegrityProof, out var resolvedAlg, out var resolvedDigest) ||
                        !integrityAlgorithm.Equals(resolvedAlg, StringComparison.OrdinalIgnoreCase) ||
                        !resolvedDigest.AsSpan().SequenceEqual(integrityDigest))
                    {
                        context.AddError("verifier.storage.integrity_digest_mismatch", "Storage provider digest does not match Codex Entry integrity proof.");
                    }
                }

                if (!string.Equals(metadata.Descriptor.Location.Region, entry.Storage.Location.Region, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(metadata.Descriptor.Location.Jurisdiction, entry.Storage.Location.Jurisdiction, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(metadata.Descriptor.Location.Provider, entry.Storage.Location.Provider, StringComparison.Ordinal))
                {
                    context.AddWarning("verifier.storage.location_metadata_mismatch", "Storage location metadata differs from declared values.");
                }
            }
            catch (StorageAdapterException ex)
            {
                context.AddError("verifier.storage.adapter_error", $"Storage adapter reported an error: {ex.Message}");
            }
        }, cancellationToken).ConfigureAwait(false);

        await RunStepAsync("Anchor validation", result, async context =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (entry.Anchor is null)
            {
                context.AddError("verifier.anchor.missing", "Anchor proof is required for verification.", "anchor");
                return;
            }

            if (_anchorService is null)
            {
                context.AddError("verifier.anchor.service_missing", "No anchor service has been configured.");
                return;
            }

            if (anchorHash is null || string.IsNullOrWhiteSpace(anchorHashAlgorithm))
            {
                context.AddError("verifier.anchor.hash_unavailable", "Anchor hash could not be computed during canonicalization.");
                return;
            }

            var network = ResolveAnchorNetwork(entry.Anchor);
            if (string.IsNullOrWhiteSpace(network))
            {
                context.AddError("verifier.anchor.network_unresolved", "Unable to resolve anchor network for verification.", "anchor.chain");
                return;
            }

            context.AddMetadata("anchor_network", network);
            try
            {
                if (!await _anchorService.VerifyAnchorAsync(entry.Anchor, entry, network!, null, cancellationToken).ConfigureAwait(false))
                {
                    context.AddError("verifier.anchor.invalid", "Anchor proof could not be verified on the configured network.");
                }
            }
            catch (Exception ex)
            {
                context.AddError("verifier.anchor.exception", $"Anchor verification threw an exception: {ex.Message}");
            }
        }, cancellationToken).ConfigureAwait(false);

        await RunStepAsync("Encryption policy validation", result, async context =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (entry.Encryption is null)
            {
                context.Skip("verifier.encryption.not_applicable", "Entry is not encrypted; skipping encryption policy checks.");
                return;
            }

            context.AddMetadata("key_ownership", entry.Encryption.KeyOwnership);

            if (string.Equals(entry.Encryption.KeyOwnership, "multi-sig", StringComparison.OrdinalIgnoreCase))
            {
                if (entry.Encryption.Policy?.Threshold is int threshold && threshold > validSignatureKeyIds.Count)
                {
                    context.AddError("verifier.encryption.threshold_mismatch", $"Encryption policy threshold {threshold} exceeds valid signer count {validSignatureKeyIds.Count}.");
                }

                var missing = entry.Encryption.LastControlledBy?.Where(id => !validSignatureKeyIds.Contains(id)).ToList();
                if (missing is not null && missing.Count > 0)
                {
                    context.AddError("verifier.encryption.control_mismatch", $"last_controlled_by keys lack corresponding valid signatures: {string.Join(", ", missing)}.");
                }
            }
            else if (entry.Encryption.LastControlledBy is not null && entry.Encryption.LastControlledBy.Count > 0)
            {
                var unmatched = entry.Encryption.LastControlledBy.Where(id => !validSignatureKeyIds.Contains(id)).ToList();
                if (unmatched.Count > 0)
                {
                    context.AddWarning("verifier.encryption.unmatched_keys", $"Some last_controlled_by keys were not observed in valid signatures: {string.Join(", ", unmatched)}.");
                }
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);

        await RunStepAsync("Revision chain validation", result, async context =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(entry.PreviousId))
            {
                context.Skip("verifier.revision.not_applicable", "Entry does not declare a previous revision.");
                return;
            }

            if (_revisionResolver is null)
            {
                context.AddWarning("verifier.revision.resolver_missing", "No revision resolver configured; unable to traverse revision chain.");
                return;
            }

            var traversal = _revisionGraph.Traverse(entry, id => _revisionResolver(id), _maxRevisionDepth);
            foreach (var issue in traversal.Issues)
            {
                if (issue.Severity == RevisionIssueSeverity.Error)
                {
                    context.AddError(issue.Code, issue.Message, issue.EntryId);
                }
                else
                {
                    context.AddWarning(issue.Code, issue.Message, issue.EntryId);
                }
            }

            context.AddMetadata("revision_depth", traversal.Chain.Count.ToString(CultureInfo.InvariantCulture));
            await Task.CompletedTask.ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);

        return result;
    }

    public async Task<VerificationResult> VerifyCertificateAsync(CertificateDescriptor certificate, CodexEntry entry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(certificate);
        ArgumentNullException.ThrowIfNull(entry);

        if (_certificateService is null)
        {
            throw new InvalidOperationException("Certificate service has not been configured for the verifier.");
        }

        var result = new VerificationResult();
        await RunStepAsync("Certificate validation", result, async context =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var validation = await _certificateService.ValidateCertificateAsync(certificate, entry, cancellationToken).ConfigureAwait(false);
            if (!validation.Success)
            {
                foreach (var error in validation.Errors)
                {
                    context.AddError("verifier.certificate.error", error);
                }
            }

            foreach (var warning in validation.Warnings)
            {
                context.AddWarning("verifier.certificate.warning", warning);
            }
        }, cancellationToken).ConfigureAwait(false);

        return result;
    }

    public Task<RevisionChainResult> TraverseRevisionChainAsync(string entryId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entryId);
        cancellationToken.ThrowIfCancellationRequested();

        if (_revisionResolver is null)
        {
            throw new InvalidOperationException("Revision resolver has not been configured for the verifier.");
        }

        var head = _revisionResolver(entryId) ?? throw new InvalidOperationException($"Codex entry '{entryId}' could not be resolved.");
        var traversal = _revisionGraph.Traverse(head, id => _revisionResolver(id), _maxRevisionDepth);
        return Task.FromResult(RevisionChainResult.FromTraversal(traversal));
    }

    private async Task RunStepAsync(string name, VerificationResult result, Func<VerificationStepContext, Task> step, CancellationToken cancellationToken)
    {
        var stepResult = new VerificationStepResult(name);
        result.AddStep(stepResult);
        var context = new VerificationStepContext(stepResult);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await step(context).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            context.Skip("verifier.step.cancelled", $"Verification step '{name}' was cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            context.AddError("verifier.step.exception", $"Unhandled exception during '{name}': {ex.Message}");
        }
        finally
        {
            if (!context.IsSkipped)
            {
                stepResult.FinalizeStatus();
            }
        }
    }

    private static int DetermineRequiredSignatures(CodexEntry entry)
    {
        if (entry.Encryption is not null && entry.Encryption.Policy?.Threshold is int threshold && threshold > 0)
        {
            return threshold;
        }

        return 1;
    }

    private static bool TryResolveHashAlgorithm(string? value, out HashAlgorithmName algorithm)
    {
        algorithm = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var normalized = value.Replace("-", string.Empty, StringComparison.OrdinalIgnoreCase).ToLowerInvariant();
        return normalized switch
        {
            "sha256" => Assign(HashAlgorithmName.SHA256, out algorithm),
            "sha384" => Assign(HashAlgorithmName.SHA384, out algorithm),
            "sha512" => Assign(HashAlgorithmName.SHA512, out algorithm),
            _ => false
        };
    }

    private static bool Assign(HashAlgorithmName name, out HashAlgorithmName algorithm)
    {
        algorithm = name;
        return true;
    }

    private string? ResolveAnchorNetwork(AnchorProof anchor)
    {
        if (_anchorNetworkResolver is not null)
        {
            return _anchorNetworkResolver(anchor);
        }

        if (!string.IsNullOrWhiteSpace(anchor.Chain))
        {
            var parts = anchor.Chain.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                return parts[^1];
            }
        }

        return _defaultAnchorNetwork;
    }
}
