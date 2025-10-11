using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Lockb0x.Anchor.Stellar;
using Lockb0x.Certificates;
using Lockb0x.Core.Canonicalization;
using Lockb0x.Core.Models;
using Lockb0x.Core.Validation;
using Lockb0x.Signing;
using Lockb0x.Storage;
using Lockb0x.Verifier;
using Lockb0x.Core.Utilities;
using Lockb0x.Certificates.Models;

namespace Lockb0x.Tests;


public class VerifierServiceTests
{
    [Fact]
    public async Task VerifyAsync_Demonstrates_Full_Protocol_Pipeline_With_Mocks()
    {
        // Sample Codex Entry (IPFS + Stellar) based on /spec/appendix-a-flows.md
        var canonicalizer = new JcsCanonicalizer();
        var validator = new CodexEntryValidator();
        var keyStore = new InMemoryKeyStore();
        var signingKey = CreateEd25519Key("did:example:org", "signer-demo");
        await keyStore.AddKeyAsync(signingKey);
        var signingService = new JoseCoseSigningService(keyStore);
        var timestamp = DateTimeOffset.UtcNow;
        var storage = new StorageDescriptor
        {
            Protocol = "ipfs",
            IntegrityProof = "ni:///sha-256;b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9",
            Location = new StorageLocation { Region = "us-east", Jurisdiction = "US", Provider = "IPFS" },
            MediaType = "application/pdf",
            SizeBytes = 12345
        };
        var identity = new IdentityDescriptor
        {
            Org = "did:example:org",
            Process = "did:example:org:subordinate",
            Artifact = "EmployeeHandbook-v1"
        };
        var anchor = new AnchorProof
        {
            Chain = "stellar:testnet",
            Reference = "abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789",
            HashAlgorithm = "SHA256",
            AnchoredAt = timestamp
        };
        var encryption = CreateEncryptionDescriptor(signingKey.KeyId);
        // Build entry with placeholder signature for canonicalization
        var placeholderSignature = new string('0', 86); // Ed25519 signature is 64 bytes, base64url is up to 86 chars
        var entryId = Guid.NewGuid();
        var unsignedEntry = BuildEntry(anchor, storage, encryption, identity, timestamp, signingKey.KeyId, placeholderSignature, entryId);
        var signingPayload = CodexEntryCanonicalPayload.CreatePayload(canonicalizer, unsignedEntry);
        Console.WriteLine("Signing canonical payload:");
        Console.WriteLine(Encoding.UTF8.GetString(signingPayload));
        var signature = await signingService.SignAsync(signingPayload, signingKey, signingKey.Type);
        // Attach real signature to entry
        var entry = BuildEntry(anchor, storage, encryption, identity, timestamp, signingKey.KeyId, signature.Signature, entryId);
        var verificationPayload = CodexEntryCanonicalPayload.CreatePayload(canonicalizer, entry);
        Console.WriteLine("Verification canonical payload:");
        Console.WriteLine(Encoding.UTF8.GetString(verificationPayload));

        var storageLocation = "ipfs://QmT78zSuBmuS4z925WZfrqQ1qHaJ56DQaTfyMUF7F8ff5o";
        var storageAdapter = new StubStorageAdapter(entry.Storage, storageLocation);
        var anchorHash = CodexEntryCanonicalPayload.CreateHash(canonicalizer, entry, System.Security.Cryptography.HashAlgorithmName.SHA256);
        var anchorService = new StubAnchorService(canonicalizer, anchorHash, entry.Anchor);

        var dependencies = new VerifierDependencies
        {
            Validator = validator,
            Canonicalizer = canonicalizer,
            SigningService = signingService,
            AnchorService = anchorService,
            StorageAdapters = new Dictionary<string, IStorageAdapter>(StringComparer.OrdinalIgnoreCase)
            {
                [entry.Storage.Protocol] = storageAdapter
            },
            StorageLocationResolver = (e, _) => Task.FromResult<string?>(storageLocation),
            DefaultAnchorNetwork = "testnet"
        };


        var verifier = new VerifierService(dependencies);
        var result = await verifier.VerifyAsync(entry);

        // Assert all major pipeline steps succeeded
        if (!result.IsValid || result.Errors.Count > 0)
        {
            var errorDetails = string.Join("\n", result.Errors.Select(e => $"{e.Code}: {e.Message}"));
            throw new Xunit.Sdk.XunitException($"Verification failed. Errors:\n{errorDetails}");
        }
        Assert.Contains(result.Steps, step => step.Name == "Schema validation" && step.Status == VerificationStepStatus.Succeeded);
        Assert.Contains(result.Steps, step => step.Name == "Canonicalization" && step.Status == VerificationStepStatus.Succeeded);
        Assert.Contains(result.Steps, step => step.Name == "Signature validation" && step.Status == VerificationStepStatus.Succeeded);
        Assert.Contains(result.Steps, step => step.Name == "Integrity proof validation" && step.Status == VerificationStepStatus.Succeeded);
        Assert.Contains(result.Steps, step => step.Name == "Storage verification" && step.Status == VerificationStepStatus.Succeeded);
        Assert.Contains(result.Steps, step => step.Name == "Anchor validation" && step.Status == VerificationStepStatus.Succeeded);
        // Encryption and revision chain may be skipped if not present
        Assert.Contains(result.Steps, step => step.Name == "Encryption policy validation");
        Assert.Contains(result.Steps, step => step.Name == "Revision chain validation");
    }

    [Fact]
    public async Task VerifyAsync_ReturnsSuccess_ForValidEntry()
    {
        var canonicalizer = new JcsCanonicalizer();
        var validator = new CodexEntryValidator();
        var keyStore = new InMemoryKeyStore();
        var signingKey = CreateEd25519Key("did:example:org", "signer-demo");
        await keyStore.AddKeyAsync(signingKey);
        var signingService = new JoseCoseSigningService(keyStore);
        var timestamp = DateTimeOffset.UtcNow;
        var storage = new StorageDescriptor
        {
            Protocol = "ipfs",
            IntegrityProof = "ni:///sha-256;b94d27b9934d3e08a52e52d7da7dabfac484efe37a5380ee9088f7ace2efcde9",
            Location = new StorageLocation { Region = "us-east", Jurisdiction = "US", Provider = "IPFS" },
            MediaType = "application/pdf",
            SizeBytes = 12345
        };
        var identity = new IdentityDescriptor
        {
            Org = "did:example:org",
            Process = "did:example:org:subordinate",
            Artifact = "EmployeeHandbook-v1"
        };
        var anchor = new AnchorProof
        {
            Chain = "stellar:testnet",
            Reference = "abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789",
            HashAlgorithm = "SHA256",
            AnchoredAt = timestamp
        };
        var encryption = CreateEncryptionDescriptor(signingKey.KeyId);
        var placeholderSignature = new string('0', 86);
        var entryId = Guid.NewGuid();
        var unsignedEntry = BuildEntry(anchor, storage, encryption, identity, timestamp, signingKey.KeyId, placeholderSignature, entryId);
        var signingPayload = CodexEntryCanonicalPayload.CreatePayload(canonicalizer, unsignedEntry);
        var signature = await signingService.SignAsync(signingPayload, signingKey, signingKey.Type);
        var entry = BuildEntry(anchor, storage, encryption, identity, timestamp, signingKey.KeyId, signature.Signature, entryId);
        var storageLocation = "ipfs://QmT78zSuBmuS4z925WZfrqQ1qHaJ56DQaTfyMUF7F8ff5o";
        var storageAdapter = new StubStorageAdapter(entry.Storage, storageLocation);
        var anchorHash = CodexEntryCanonicalPayload.CreateHash(canonicalizer, entry, System.Security.Cryptography.HashAlgorithmName.SHA256);
        var anchorService = new StubAnchorService(canonicalizer, anchorHash, entry.Anchor);
        var dependencies = new VerifierDependencies
        {
            Validator = validator,
            Canonicalizer = canonicalizer,
            SigningService = signingService,
            AnchorService = anchorService,
            StorageAdapters = new Dictionary<string, IStorageAdapter>(StringComparer.OrdinalIgnoreCase)
            {
                [entry.Storage.Protocol] = storageAdapter
            },
            StorageLocationResolver = (e, _) => Task.FromResult<string?>(storageLocation),
            DefaultAnchorNetwork = "testnet"
        };
        var verifier = new VerifierService(dependencies);
        var result = await verifier.VerifyAsync(entry);
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.Contains(result.Steps, step => step.Name == "Signature validation" && step.Status == VerificationStepStatus.Succeeded);
    }

    [Fact]
    public async Task VerifyCertificateAsync_DelegatesToCertificateService()
    {
        var canonicalizer = new JcsCanonicalizer();
        var validator = new CodexEntryValidator();
        var signingService = new JoseCoseSigningService();
        var signingKey = CreateEd25519Key("did:example:org", "signer-1");
        var timestamp = DateTimeOffset.UtcNow;
        var storage = CreateStorageDescriptor();
        var encryption = CreateEncryptionDescriptor(signingKey.KeyId);
        var identity = CreateIdentityDescriptor();
        var anchor = CreateAnchorProof(timestamp, canonicalizer, storage, encryption, identity, signingKey.KeyId);
        // Build base entry with a placeholder signature to canonicalize
        var baseEntry = BuildEntry(anchor, storage, encryption, identity, timestamp, signingKey.KeyId, "placeholder");
        var payload = CodexEntryCanonicalPayload.CreatePayload(canonicalizer, baseEntry);
        var signature = await signingService.SignAsync(payload, signingKey, signingKey.Type);
        var entry = BuildEntry(anchor, storage, encryption, identity, timestamp, signingKey.KeyId, signature.Signature, Guid.NewGuid());

        var certificate = new CertificateDescriptor("urn:uuid:test", entry.Id, entry.Identity.Org, entry.Identity.Subject ?? entry.Id, CertificatePurpose.Attestation, timestamp, null, CertificateStatus.Active, Array.Empty<CertificateRepresentation>(), Array.Empty<CertificateEvent>());
        var certificateService = new StubCertificateService(success: true);

        var dependencies = new VerifierDependencies
        {
            Validator = validator,
            Canonicalizer = canonicalizer,
            SigningService = signingService,
            CertificateService = certificateService
        };

        var verifier = new VerifierService(dependencies);
        var result = await verifier.VerifyCertificateAsync(certificate, entry);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task TraverseRevisionChainAsync_ReturnsEntries_FromResolver()
    {
        var canonicalizer = new JcsCanonicalizer();
        var validator = new CodexEntryValidator();
        var signingService = new JoseCoseSigningService();
        var signingKey = CreateEd25519Key("did:example:org", "signer-1");
        var timestamp = DateTimeOffset.UtcNow;
        var storage = CreateStorageDescriptor();
        var encryption = CreateEncryptionDescriptor(signingKey.KeyId);
        var identity = CreateIdentityDescriptor();
        var anchor = new AnchorProof
        {
            Chain = "stellar:testnet",
            Reference = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef",
            HashAlgorithm = "SHA256",
            AnchoredAt = timestamp
        };

        var previous = BuildEntry(anchor, storage, encryption, identity, timestamp, signingKey.KeyId, Convert.ToBase64String(Encoding.UTF8.GetBytes("sig")), Guid.NewGuid());
        var head = BuildEntry(anchor, storage, encryption, identity, timestamp, signingKey.KeyId, Convert.ToBase64String(Encoding.UTF8.GetBytes("sig")), Guid.NewGuid(), previous.Id);

        var resolver = new Dictionary<string, CodexEntry>(StringComparer.Ordinal)
        {
            [head.Id] = head,
            [previous.Id] = previous
        };

        var dependencies = new VerifierDependencies
        {
            Validator = validator,
            Canonicalizer = canonicalizer,
            SigningService = signingService,
            RevisionResolver = id => resolver.TryGetValue(id, out var entry) ? entry : null
        };

        var verifier = new VerifierService(dependencies);
        var result = await verifier.TraverseRevisionChainAsync(head.Id);

        Assert.True(result.Success);
        Assert.Equal(2, result.Entries.Count);
    }

    private static StorageDescriptor CreateStorageDescriptor()
    {
        return new StorageDescriptor
        {
            Protocol = "ipfs",
            IntegrityProof = NiUri.Create(Encoding.UTF8.GetBytes("payload"), HashAlgorithmName.SHA256),
            MediaType = "application/pdf",
            SizeBytes = 1024,
            Location = new StorageLocation
            {
                Provider = "Lockb0x",
                Region = "us-central1",
                Jurisdiction = "US"
            }
        };
    }

    private static EncryptionDescriptor CreateEncryptionDescriptor(string keyId)
    {
        return new EncryptionDescriptor
        {
            Algorithm = "AES-256-GCM",
            KeyOwnership = "multi-sig",
            Policy = new EncryptionPolicy
            {
                Type = "threshold",
                Threshold = 1,
                Total = 1
            },
            PublicKeys = new[] { keyId },
            LastControlledBy = new[] { keyId }
        };
    }

    private static IdentityDescriptor CreateIdentityDescriptor()
    {
        return new IdentityDescriptor
        {
            Org = "did:example:org",
            Artifact = "Example Artifact",
            Subject = "did:example:artifact"
        };
    }

    private static AnchorProof CreateAnchorProof(DateTimeOffset timestamp, IJsonCanonicalizer canonicalizer, StorageDescriptor storage, EncryptionDescriptor encryption, IdentityDescriptor identity, string keyId)
    {
        var placeholderAnchor = new AnchorProof
        {
            Chain = "stellar:testnet",
            Reference = "placeholder",
            HashAlgorithm = "SHA256",
            AnchoredAt = timestamp
        };

        var placeholderEntry = BuildEntry(placeholderAnchor, storage, encryption, identity, timestamp, keyId, "placeholder");
        var payload = CodexEntryCanonicalPayload.CreatePayload(canonicalizer, placeholderEntry);
        var hash = Convert.ToHexString(SHA256.HashData(payload)).ToLowerInvariant();

        return new AnchorProof
        {
            Chain = placeholderAnchor.Chain,
            Reference = hash,
            HashAlgorithm = placeholderAnchor.HashAlgorithm,
            AnchoredAt = placeholderAnchor.AnchoredAt
        };
    }

    private static CodexEntry BuildEntry(AnchorProof anchor, StorageDescriptor storage, EncryptionDescriptor encryption, IdentityDescriptor identity, DateTimeOffset timestamp, string keyId, string signatureValue, Guid? id = null, string? previousId = null)
    {
        var builder = new CodexEntryBuilder()
            .WithId(id ?? Guid.NewGuid())
            .WithVersion("1.0.0")
            .WithStorage(storage)
            .WithEncryption(encryption)
            .WithIdentity(identity)
            .WithTimestamp(timestamp)
            .WithAnchor(anchor);

        if (string.IsNullOrEmpty(signatureValue))
        {
            throw new ArgumentException("Signature value must be provided for entry construction.");
        }
        builder.WithSignatures(new[]
        {
            new SignatureProof
            {
                Protected = new SignatureProtectedHeader
                {
                    Algorithm = "EdDSA",
                    KeyId = keyId
                },
                Signature = signatureValue
            }
        });

        if (!string.IsNullOrWhiteSpace(previousId))
        {
            builder.WithPreviousId(previousId);
        }

        return builder.Build();
    }

    private static SigningKey CreateEd25519Key(string controller, string keyId)
    {
        var privateKeyHex = "9d61b19deffd5a60ba844af492ec2cc44449c5697b326919703bac031cae7f60";
        var publicKeyHex = "d75a980182b10ab7d54bfed3c964073a0ee172f3daa62325af021a68f707511a";

        return new SigningKey
        {
            KeyId = keyId,
            Type = "Ed25519",
            Controller = controller,
            PrivateKey = Convert.ToBase64String(Convert.FromHexString(privateKeyHex)),
            PublicKey = Convert.ToBase64String(Convert.FromHexString(publicKeyHex))
        };
    }

    internal sealed class StubStorageAdapter : IStorageAdapter
    {
        private readonly StorageDescriptor _descriptor;
        private readonly string _location;

        public StubStorageAdapter(StorageDescriptor descriptor, string location)
        {
            _descriptor = descriptor;
            _location = location;
        }

        public Task<StorageResult> StoreAsync(StorageUploadRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Stream> FetchAsync(string location, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<bool> ExistsAsync(string location, CancellationToken cancellationToken = default)
            => Task.FromResult(string.Equals(location, _location, StringComparison.Ordinal));

        public Task<StorageResult> GetMetadataAsync(string location, CancellationToken cancellationToken = default)
        {
            if (!string.Equals(location, _location, StringComparison.Ordinal))
            {
                throw new StorageAdapterException("Resource not found.");
            }

            return Task.FromResult(new StorageResult
            {
                Descriptor = _descriptor,
                ContentIdentifier = location,
                ResourceUri = location
            });
        }
    }

    internal sealed class StubAnchorService : IStellarAnchorService
    {
        private readonly IJsonCanonicalizer _canonicalizer;
        private readonly byte[] _expectedHash;
        private readonly AnchorProof _anchor;

        public StubAnchorService(IJsonCanonicalizer canonicalizer, byte[] expectedHash, AnchorProof anchor)
        {
            _canonicalizer = canonicalizer;
            _expectedHash = expectedHash;
            _anchor = anchor;
        }

        public Task<AnchorProof> AnchorAsync(CodexEntry entry, string network = "testnet", string? stellarPublicKey = null, CancellationToken cancellationToken = default)
            => Task.FromResult(_anchor);

        public Task<string> GetTransactionUrlAsync(AnchorProof anchor, string network = "testnet", CancellationToken cancellationToken = default)
            => Task.FromResult($"https://stellar.example/{anchor.Reference}");

        public Task<bool> VerifyAnchorAsync(AnchorProof anchor, CodexEntry entry, string network = "testnet", string? stellarPublicKey = null, CancellationToken cancellationToken = default)
        {
            if (!string.Equals(network, "testnet", StringComparison.Ordinal))
            {
                return Task.FromResult(false);
            }

            var payload = CodexEntryCanonicalPayload.CreatePayload(_canonicalizer, entry);
            var hash = SHA256.HashData(payload);
            return Task.FromResult(anchor.Reference.Equals(_anchor.Reference, StringComparison.OrdinalIgnoreCase) && hash.AsSpan().SequenceEqual(_expectedHash));
        }
    }

    internal sealed class StubCertificateService : ICertificateService
    {
        private readonly bool _success;

        public StubCertificateService(bool success) => _success = success;

        public Task<CertificateDescriptor?> GetCertificateAsync(string certificateId, CancellationToken cancellationToken = default)
            => Task.FromResult<CertificateDescriptor?>(null);

        public Task<CertificateDescriptor> IssueCertificateAsync(CodexEntry entry, CertificateOptions options, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<bool> RevokeCertificateAsync(string certificateId, string? reason = null, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<CertificateValidationResult> ValidateCertificateAsync(CertificateDescriptor certificate, CodexEntry entry, CancellationToken cancellationToken = default)
        {
            var result = new CertificateValidationResult();
            if (!_success)
            {
                result.AddError("validation failed");
            }

            return Task.FromResult(result);
        }
    }
}
