using Lockb0x.Certificates;
using Lockb0x.Core;
using Xunit;
using System;
using System.Threading.Tasks;

namespace Lockb0x.Tests;

public class CertificateServiceTests
{

    [Fact]
    public async Task CertificateService_Issues_And_Verifies_Certificate()
    {
        var service = new CertificateService();
        var entry = new CodexEntry { Id = "did:example:subject" };
        var cert = await service.IssueCertificateAsync(entry);
        Assert.False(string.IsNullOrEmpty(cert.Id));
        Assert.Equal("did:example:issuer", cert.Issuer);
        Assert.Equal("did:example:subject", cert.Subject);
        Assert.False(string.IsNullOrEmpty(cert.Jws));
        Assert.True(cert.IssuedAt <= DateTimeOffset.UtcNow);
        var verified = await service.VerifyCertificateAsync(cert, entry);
        Assert.True(verified);
    }

    [Fact]
    public async Task CertificateService_VerifyCertificate_Fails_On_Invalid_Jws()
    {
        var service = new CertificateService();
        var entry = new CodexEntry { Id = "did:example:subject" };
        var cert = new Certificate { Subject = entry.Id, Jws = "" };
        var verified = await service.VerifyCertificateAsync(cert, entry);
        Assert.False(verified);
    }

    [Fact]
    public async Task CertificateService_VerifyCertificate_Fails_On_Subject_Mismatch()
    {
        var service = new CertificateService();
        var entry = new CodexEntry { Id = "did:example:subject" };
        var cert = new Certificate { Subject = "did:example:other", Jws = "stub-jws" };
        var verified = await service.VerifyCertificateAsync(cert, entry);
        Assert.False(verified);
    }
}
