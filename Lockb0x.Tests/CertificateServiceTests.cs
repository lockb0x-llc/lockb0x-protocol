using Lockb0x.Certificates;
using Lockb0x.Core;
using Xunit;
using System;
using System.Threading.Tasks;

namespace Lockb0x.Tests;

public class CertificateServiceTests
{
    [Fact]
    public async Task CertificateService_Throws_NotImplemented()
    {
        var service = new CertificateService();
        var entry = new CodexEntry();
        var cert = new Certificate();
        await Assert.ThrowsAsync<NotImplementedException>(() => service.IssueCertificateAsync(entry));
        await Assert.ThrowsAsync<NotImplementedException>(() => service.VerifyCertificateAsync(cert, entry));
    }
}
