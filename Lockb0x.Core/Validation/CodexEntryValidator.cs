using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using Lockb0x.Core.Models;
using Lockb0x.Core.Utilities;

namespace Lockb0x.Core.Validation;

public sealed class CodexEntryValidator : ICodexEntryValidator
{
    private static readonly Regex MediaTypePattern = new("^[a-z0-9!#$&^_.+-]+/[a-z0-9!#$&^_.+-]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex StellarHashPattern = new("^[0-9a-f]{64}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public ValidationResult Validate(CodexEntry entry, CodexEntryValidationContext? context = null)
    {
        if (entry is null) throw new ArgumentNullException(nameof(entry));

        var errors = new List<CodexEntryValidationError>();
        var warnings = new List<CodexEntryValidationWarning>();

        ValidateIdentity(entry.Identity, errors);
        ValidateId(entry.Id, errors, "id");
        if (!string.IsNullOrWhiteSpace(entry.PreviousId))
        {
            ValidateId(entry.PreviousId!, errors, "previous_id");
        }

        if (string.IsNullOrWhiteSpace(entry.Version))
        {
            errors.Add(new("core.validation.missing_field", "version is required", "version"));
        }

        ValidateStorage(entry.Storage, errors);
        ValidateEncryption(entry.Encryption, errors);
        ValidateTimestamp(entry.Timestamp, errors);
        ValidateAnchor(entry.Anchor, context, errors);
        ValidateSignatures(entry.Signatures, errors);

        return errors.Count == 0 && warnings.Count == 0
            ? ValidationResult.SuccessResult
            : new ValidationResult(errors, warnings);
    }

    private static void ValidateId(string value, List<CodexEntryValidationError> errors, string path)
    {
        if (!Guid.TryParse(value, out var guid))
        {
            errors.Add(new("core.validation.invalid_guid", $"{path} must be a RFC 4122 UUID", path));
            return;
        }

        var version = (guid.ToByteArray()[7] >> 4) & 0x0F;
        if (version != 4)
        {
            errors.Add(new("core.validation.invalid_guid_version", $"{path} must be a UUID v4", path));
        }
    }

    private static void ValidateStorage(StorageDescriptor storage, List<CodexEntryValidationError> errors)
    {
        if (storage is null)
        {
            errors.Add(new("core.validation.missing_field", "storage is required", "storage"));
            return;
        }

        if (string.IsNullOrWhiteSpace(storage.Protocol))
        {
            errors.Add(new("core.validation.missing_field", "storage.protocol is required", "storage.protocol"));
        }

        if (string.IsNullOrWhiteSpace(storage.IntegrityProof))
        {
            errors.Add(new("core.validation.missing_field", "storage.integrity_proof is required", "storage.integrity_proof"));
        }
        else if (!NiUri.TryParse(storage.IntegrityProof, out _, out _))
        {
            errors.Add(new("core.storage.invalid_integrity_proof", "storage.integrity_proof must be an RFC 6920 ni-URI", "storage.integrity_proof"));
        }

        if (string.IsNullOrWhiteSpace(storage.MediaType) || !MediaTypePattern.IsMatch(storage.MediaType))
        {
            errors.Add(new("core.storage.invalid_media_type", "storage.media_type must be a valid RFC 6838 media type", "storage.media_type"));
        }

        if (storage.SizeBytes <= 0)
        {
            errors.Add(new("core.storage.invalid_size", "storage.size_bytes must be a positive integer", "storage.size_bytes"));
        }

        if (storage.Location is null)
        {
            errors.Add(new("core.validation.missing_field", "storage.location is required", "storage.location"));
        }
        else
        {
            if (string.IsNullOrWhiteSpace(storage.Location.Region))
            {
                errors.Add(new("core.storage.missing_region", "storage.location.region is required", "storage.location.region"));
            }

            if (string.IsNullOrWhiteSpace(storage.Location.Jurisdiction))
            {
                errors.Add(new("core.storage.missing_jurisdiction", "storage.location.jurisdiction is required", "storage.location.jurisdiction"));
            }

            if (string.IsNullOrWhiteSpace(storage.Location.Provider))
            {
                errors.Add(new("core.storage.missing_provider", "storage.location.provider is required", "storage.location.provider"));
            }
        }
    }

    private static void ValidateEncryption(EncryptionDescriptor? encryption, List<CodexEntryValidationError> errors)
    {
        if (encryption is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(encryption.Algorithm))
        {
            errors.Add(new("core.encryption.missing_algorithm", "encryption.algorithm is required when encryption is present", "encryption.algorithm"));
        }

        if (string.IsNullOrWhiteSpace(encryption.KeyOwnership))
        {
            errors.Add(new("core.encryption.missing_key_ownership", "encryption.key_ownership is required", "encryption.key_ownership"));
        }

        if (encryption.LastControlledBy is null || encryption.LastControlledBy.Count == 0)
        {
            errors.Add(new("core.encryption.missing_last_controlled_by", "encryption.last_controlled_by must list at least one key", "encryption.last_controlled_by"));
        }
        else
        {
            var index = 0;
            foreach (var key in encryption.LastControlledBy)
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    errors.Add(new("core.encryption.invalid_last_controlled_by", "last_controlled_by entries must be non-empty identifiers", $"encryption.last_controlled_by[{index}]"));
                }
                index++;
            }
        }

        if (encryption.PublicKeys is not null)
        {
            var index = 0;
            foreach (var key in encryption.PublicKeys)
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    errors.Add(new("core.encryption.invalid_public_key", "public_keys entries must be non-empty", $"encryption.public_keys[{index}]"));
                }
                index++;
            }
        }

        if (encryption.Policy is not null)
        {
            if (string.IsNullOrWhiteSpace(encryption.Policy.Type))
            {
                errors.Add(new("core.encryption.invalid_policy", "encryption.policy.type is required when policy is provided", "encryption.policy.type"));
            }

            if (encryption.Policy.Type.Equals("threshold", StringComparison.OrdinalIgnoreCase))
            {
                if (encryption.Policy.Threshold is null || encryption.Policy.Threshold <= 0)
                {
                    errors.Add(new("core.encryption.invalid_policy_threshold", "threshold policies must declare a positive threshold", "encryption.policy.threshold"));
                }

                if (encryption.Policy.Total is null || encryption.Policy.Total <= 0)
                {
                    errors.Add(new("core.encryption.invalid_policy_total", "threshold policies must declare a positive total", "encryption.policy.total"));
                }
                else if (encryption.Policy.Threshold is not null && encryption.Policy.Total < encryption.Policy.Threshold)
                {
                    errors.Add(new("core.encryption.invalid_policy_threshold", "threshold cannot exceed total participants", "encryption.policy"));
                }
            }
        }
    }

    private static void ValidateIdentity(IdentityDescriptor identity, List<CodexEntryValidationError> errors)
    {
        if (identity is null)
        {
            errors.Add(new("core.validation.missing_field", "identity is required", "identity"));
            return;
        }

        if (!IsValidIdentity(identity.Org))
        {
            errors.Add(new("core.identity.invalid_org", "identity.org must be a Stellar account or DID", "identity.org"));
        }

        if (!string.IsNullOrWhiteSpace(identity.Process) && !IsValidIdentity(identity.Process!))
        {
            errors.Add(new("core.identity.invalid_process", "identity.process must be a subordinate Stellar account or DID", "identity.process"));
        }

        if (string.IsNullOrWhiteSpace(identity.Artifact))
        {
            errors.Add(new("core.identity.missing_artifact", "identity.artifact is required", "identity.artifact"));
        }

        if (!string.IsNullOrWhiteSpace(identity.Subject) && !IsValidIdentity(identity.Subject!))
        {
            errors.Add(new("core.identity.invalid_subject", "identity.subject must be expressed as a DID or account identifier", "identity.subject"));
        }
    }

    private static bool IsValidIdentity(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        if (value.StartsWith("did:", StringComparison.Ordinal)) return true;
        if (value.StartsWith("stellar:", StringComparison.Ordinal))
        {
            var account = value.Split(':', 2)[^1];
            return account.Length == 56 && account.StartsWith('G');
        }

        if (value.Contains(':', StringComparison.Ordinal) && value.Contains('/'))
        {
            // Allow CAIP-10 accounts (e.g., chain:namespace:address)
            return true;
        }

        return false;
    }

    private static void ValidateTimestamp(DateTimeOffset timestamp, List<CodexEntryValidationError> errors)
    {
        if (timestamp.Offset != TimeSpan.Zero)
        {
            errors.Add(new("core.validation.timestamp_not_utc", "timestamp must be expressed in UTC", "timestamp"));
        }
    }

    private static void ValidateAnchor(AnchorProof anchor, CodexEntryValidationContext? context, List<CodexEntryValidationError> errors)
    {
        if (anchor is null)
        {
            errors.Add(new("core.validation.missing_field", "anchor is required", "anchor"));
            return;
        }

        if (string.IsNullOrWhiteSpace(anchor.Chain))
        {
            errors.Add(new("core.anchor.missing_chain", "anchor.chain is required", "anchor.chain"));
        }
        else
        {
            if (!IsValidAnchorChain(anchor.Chain))
            {
                errors.Add(new("core.anchor.invalid_chain", "anchor.chain must be a valid CAIP-2 identifier or supported anchor type", "anchor.chain"));
            }

            if (!string.IsNullOrWhiteSpace(context?.Network) && !string.Equals(context.Network, anchor.Chain, StringComparison.Ordinal))
            {
                errors.Add(new("core.anchor.network_mismatch", $"anchor.chain '{anchor.Chain}' does not match configured network '{context!.Network}'", "anchor.chain"));
            }

            if (anchor.Chain.StartsWith("stellar:", StringComparison.OrdinalIgnoreCase) && !StellarHashPattern.IsMatch(anchor.Reference))
            {
                errors.Add(new("core.anchor.invalid_anchor_ref", "stellar anchors must supply a 64-character hexadecimal anchor_ref", "anchor.anchor_ref"));
            }
        }

        if (string.IsNullOrWhiteSpace(anchor.Reference))
        {
            errors.Add(new("core.anchor.missing_anchor_ref", "anchor.anchor_ref is required", "anchor.anchor_ref"));
        }

        if (string.IsNullOrWhiteSpace(anchor.HashAlgorithm))
        {
            errors.Add(new("core.anchor.missing_hash_algorithm", "anchor.hash_alg is required", "anchor.hash_alg"));
        }
        else if (!IsSupportedHashAlgorithm(anchor.HashAlgorithm))
        {
            errors.Add(new("core.anchor.invalid_hash_algorithm", "anchor.hash_alg must be one of: SHA256, SHA3-256", "anchor.hash_alg"));
        }

        if (anchor.AnchoredAt is { } anchoredAt && anchoredAt.Offset != TimeSpan.Zero)
        {
            errors.Add(new("core.anchor.timestamp_not_utc", "anchor.anchored_at must be expressed in UTC", "anchor.anchored_at"));
        }

        // NFT anchors require contract_address
        if (!string.IsNullOrWhiteSpace(anchor.TokenId) && string.IsNullOrWhiteSpace(anchor.ContractAddress))
        {
            errors.Add(new("core.anchor.missing_contract_address", "anchor.contract_address is required when token_id is present (NFT anchors)", "anchor.contract_address"));
        }
    }

    private static void ValidateSignatures(IReadOnlyList<SignatureProof> signatures, List<CodexEntryValidationError> errors)
    {
        if (signatures is null || signatures.Count == 0)
        {
            errors.Add(new("core.signatures.missing", "At least one signature proof is required", "signatures"));
            return;
        }

        for (var i = 0; i < signatures.Count; i++)
        {
            var signature = signatures[i];
            if (signature is null)
            {
                errors.Add(new("core.signatures.invalid", "Signature entry cannot be null", $"signatures[{i}]"));
                continue;
            }

            if (signature.Protected is null)
            {
                errors.Add(new("core.signatures.missing_protected", "Signature must include protected header", $"signatures[{i}].protected"));
            }
            else
            {
                if (string.IsNullOrWhiteSpace(signature.Protected.Algorithm))
                {
                    errors.Add(new("core.signatures.missing_algorithm", "Signature protected header must declare alg", $"signatures[{i}].protected.alg"));
                }

                if (string.IsNullOrWhiteSpace(signature.Protected.KeyId))
                {
                    errors.Add(new("core.signatures.missing_kid", "Signature protected header must declare kid", $"signatures[{i}].protected.kid"));
                }
            }

            if (string.IsNullOrWhiteSpace(signature.Signature))
            {
                errors.Add(new("core.signatures.missing_value", "Signature value is required", $"signatures[{i}].signature"));
            }
        }
    }

    private static bool IsValidAnchorChain(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        if (IsSupportedNonBlockchainAnchor(value)) return true;
        return IsValidCaip2(value);
    }

    private static bool IsValidCaip2(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var parts = value.Split(':', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 2 || parts.Length == 3;
    }

    private static bool IsSupportedNonBlockchainAnchor(string value)
    {
        return value.Equals("gdrive", StringComparison.OrdinalIgnoreCase)
            || value.Equals("solid", StringComparison.OrdinalIgnoreCase)
            || value.Equals("notary", StringComparison.OrdinalIgnoreCase)
            || value.Equals("opentimestamps", StringComparison.OrdinalIgnoreCase)
            || value.Equals("rfc3161", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSupportedHashAlgorithm(string value)
    {
        return value.Equals("SHA256", StringComparison.OrdinalIgnoreCase)
            || value.Equals("SHA3-256", StringComparison.OrdinalIgnoreCase);
    }
}
