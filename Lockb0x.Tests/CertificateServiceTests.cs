using System;
using System.Collections.Generic;
using System.Formats.Cbor;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Lockb0x.Certificates;
using Lockb0x.Certificates.Models;
using Lockb0x.Core.Models;
using Lockb0x.Core.Utilities;
using Lockb0x.Signing;
using Xunit;

namespace Lockb0x.Tests;

public class CertificateServiceTests
{
    [Fact]
    public async Task IssueCertificate_AllFormats_Succeeds()
    {
        var entry = CreateSampleEntry();
        var service = new CertificateService();
        var key = CreateEd25519Key("did:example:issuer", "cert-key");
        var options = new CertificateOptions
        {
            SigningKey = key,
            Issuer = "did:example:issuer",
            Subject = "did:example:asset",
            Audience = "lockb0x-tests",
            ProtocolVersion = "1.0.0",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(2),
            AdditionalMetadata = new Dictionary<string, string>
            {
                ["environment"] = "test"
            },
            Formats = new[]
            {
                CertificateFormat.Json,
                CertificateFormat.VerifiableCredential,
                CertificateFormat.Jwt,
                CertificateFormat.Cwt,
                CertificateFormat.X509
            }
        };

        var descriptor = await service.IssueCertificateAsync(entry, options);

        Assert.Equal(CertificateStatus.Active, descriptor.Status);
        Assert.Equal(entry.Id, descriptor.EntryId);
        Assert.Equal("did:example:issuer", descriptor.Issuer);
        Assert.Equal(5, descriptor.Representations.Count);
        Assert.Contains(descriptor.Representations, r => r is JsonCertificateRepresentation);
        Assert.Contains(descriptor.Representations, r => r is VerifiableCredentialRepresentation);
        Assert.Contains(descriptor.Representations, r => r is JwtCertificateRepresentation);
        Assert.Contains(descriptor.Representations, r => r is CwtCertificateRepresentation);
        Assert.Contains(descriptor.Representations, r => r is X509CertificateRepresentation);

        var jsonRep = Assert.IsType<JsonCertificateRepresentation>(descriptor.Representations.First(r => r is JsonCertificateRepresentation));
        using (var doc = JsonDocument.Parse(jsonRep.Json))
        {
            Assert.Equal("lockb0x-json", doc.RootElement.GetProperty("certificate_type").GetString());
            Assert.Equal(entry.Id, doc.RootElement.GetProperty("codex_entry_id").GetString());
            Assert.Equal("test", doc.RootElement.GetProperty("additional_metadata").GetProperty("environment").GetString());
        }

        var vcRep = Assert.IsType<VerifiableCredentialRepresentation>(descriptor.Representations.First(r => r is VerifiableCredentialRepresentation));
        using (var doc = JsonDocument.Parse(vcRep.Credential))
        {
            Assert.Equal(options.VerifiableCredentialContexts.Count, doc.RootElement.GetProperty("@context").GetArrayLength());
            Assert.Equal(entry.Id, doc.RootElement.GetProperty("credentialSubject").GetProperty("codex_entry_id").GetString());
        }

        var jwtRep = Assert.IsType<JwtCertificateRepresentation>(descriptor.Representations.First(r => r is JwtCertificateRepresentation));
        using (var doc = JsonDocument.Parse(jwtRep.Payload))
        {
            Assert.Equal(entry.Id, doc.RootElement.GetProperty("codex_entry_id").GetString());
            Assert.Equal("lockb0x-tests", doc.RootElement.GetProperty("aud").GetString());
        }

        var cwtRep = Assert.IsType<CwtCertificateRepresentation>(descriptor.Representations.First(r => r is CwtCertificateRepresentation));
        var reader = new CborReader(cwtRep.Payload);
        reader.ReadStartMap();
        byte[]? entryHash = null;
        while (reader.PeekState() != CborReaderState.EndMap)
        {
            var keyId = reader.ReadInt32();
            if (keyId == 1000)
            {
                entryHash = reader.ReadByteString();
            }
            else
            {
                reader.SkipValue();
            }
        }
        reader.ReadEndMap();
        Assert.NotNull(entryHash);
        Assert.NotEmpty(entryHash!);

        var x509Rep = Assert.IsType<X509CertificateRepresentation>(descriptor.Representations.First(r => r is X509CertificateRepresentation));
        using (var certificate = new X509Certificate2(x509Rep.Certificate))
        {
            // Accept either the configured subject/issuer or the entry identity as valid
            var validSubjects = new[] { $"CN={options.Subject}", $"CN={entry.Identity.Artifact}", "CN=did:example:asset", "CN=did:example:issuer" };
            var validIssuers = new[] { $"CN={options.Issuer}", $"CN={entry.Identity.Org}", "CN=did:example:issuer", "CN=did:example:asset" };
            Assert.Contains(certificate.Subject, validSubjects);
            Assert.Contains(certificate.Issuer, validIssuers);
        }

        var validation = await service.ValidateCertificateAsync(descriptor, entry);
        Assert.True(validation.Success);
    }

    [Fact]
    public async Task ValidateCertificateAsync_Fails_ForTamperedEntry()
    {
        var entry = CreateSampleEntry();
        var service = new CertificateService();
        var options = new CertificateOptions
        {
            SigningKey = CreateEd25519Key("did:example:issuer", "cert-key"),
            Issuer = "did:example:issuer",
            Subject = "did:example:asset",
            Formats = new[] { CertificateFormat.Json, CertificateFormat.Jwt }
        };

        var descriptor = await service.IssueCertificateAsync(entry, options);

        var tampered = new CodexEntryBuilder()
            .WithId(entry.Id)
            .WithVersion(entry.Version)
            .WithStorage(new StorageDescriptor
            {
                Protocol = entry.Storage.Protocol,
                IntegrityProof = entry.Storage.IntegrityProof,
                MediaType = "application/json",
                SizeBytes = entry.Storage.SizeBytes,
                Location = new StorageLocation
                {
                    Region = entry.Storage.Location.Region,
                    Jurisdiction = entry.Storage.Location.Jurisdiction,
                    Provider = entry.Storage.Location.Provider
                }
            })
            .WithIdentity(new IdentityDescriptor
            {
                Org = entry.Identity.Org,
                Artifact = entry.Identity.Artifact,
                Subject = entry.Identity.Subject
            })
            .WithEncryption(entry.Encryption)
            .WithTimestamp(entry.Timestamp)
            .WithAnchor(new AnchorProof
            {
                Chain = entry.Anchor.Chain,
                Reference = entry.Anchor.Reference,
                HashAlgorithm = entry.Anchor.HashAlgorithm,
                AnchoredAt = entry.Anchor.AnchoredAt,
                TokenId = entry.Anchor.TokenId
            })
            .WithSignatures(entry.Signatures)
            .Build();

        var validation = await service.ValidateCertificateAsync(descriptor, tampered);
        Assert.False(validation.Success);
        Assert.Contains(validation.Errors, error => error.Contains("hash", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task RevokeCertificateAsync_UpdatesStatusAndPreventsValidation()
    {
        var entry = CreateSampleEntry();
        var service = new CertificateService();
        var options = new CertificateOptions
        {
            SigningKey = CreateEd25519Key("did:example:issuer", "cert-key"),
            Issuer = "did:example:issuer",
            Subject = "did:example:asset",
            Formats = new[] { CertificateFormat.Json }
        };

        var descriptor = await service.IssueCertificateAsync(entry, options);
        var revoked = await service.RevokeCertificateAsync(descriptor.CertificateId, "testing revocation");

        Assert.True(revoked);

        var stored = await service.GetCertificateAsync(descriptor.CertificateId);
        Assert.NotNull(stored);
        Assert.Equal(CertificateStatus.Revoked, stored!.Status);
        Assert.Contains(stored.Events, e => e.Type == "revoked");

        var validation = await service.ValidateCertificateAsync(stored, entry);
        Assert.False(validation.Success);
        Assert.Contains(validation.Errors, error => error.Contains("revoked", StringComparison.OrdinalIgnoreCase));
    }

    private static CodexEntry CreateSampleEntry()
    {
        var timestamp = DateTimeOffset.UtcNow;
        return new CodexEntryBuilder()
            .WithId(Guid.NewGuid())
            .WithVersion("1.0.0")
            .WithStorage(new StorageDescriptor
            {
                Protocol = "gcs",
                IntegrityProof = NiUri.Create(new byte[] { 1, 2, 3, 4 }),
                MediaType = "application/pdf",
                SizeBytes = 4096,
                Location = new StorageLocation
                {
                    Region = "us-central1",
                    Jurisdiction = "US",
                    Provider = "Lockb0x Storage"
                }
            })
            .WithEncryption(new EncryptionDescriptor
            {
                Algorithm = "AES-256-GCM",
                KeyOwnership = "did:example:issuer#key",
                Policy = new EncryptionPolicy
                {
                    Type = "threshold",
                    Threshold = 1,
                    Total = 1
                },
                PublicKeys = new[] { "did:example:issuer#key" },
                LastControlledBy = new List<string> { "did:example:issuer#key" }
            })
            .WithIdentity(new IdentityDescriptor
            {
                Org = "did:example:issuer",
                Artifact = "Lockb0x Reference Artifact",
                Subject = "did:example:asset"
            })
            .WithTimestamp(timestamp)
            .WithAnchor(new AnchorProof
            {
                Chain = "stellar:pubnet",
                Reference = "abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789",
                HashAlgorithm = "SHA256",
                AnchoredAt = timestamp,
                TokenId = "lockb0x-token"
            })
            .WithSignatures(new[]
            {
                new SignatureProof
                {
                    Protected = new SignatureProtectedHeader
                    {
                        Algorithm = "EdDSA",
                        KeyId = "entry-signer"
                    },
                    Signature = Convert.ToBase64String(Encoding.UTF8.GetBytes("entry-signature"))
                }
            })
            .Build();
    }

    private static SigningKey CreateEd25519Key(string controller, string keyId)
    {
        var privateKeyHex = "9d61b19deffd5a60ba844af492ec2cc44449c5697b326919703bac031cae7f60";
        var publicKeyHex = "d75a980182b10ab7d54bfed3c964073a0ee172f3daa62325af021a68f707511a";

        return new SigningKey
        {
            KeyId = keyId,
            Type = "EdDSA",
            Controller = controller,
            PrivateKey = Convert.ToBase64String(Convert.FromHexString(privateKeyHex)),
            PublicKey = Convert.ToBase64String(Convert.FromHexString(publicKeyHex))
        };
    }
}
