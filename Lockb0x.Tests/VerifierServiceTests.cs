using Lockb0x.Verifier;
using Lockb0x.Core;
using Xunit;
using System;
using System.Threading.Tasks;

namespace Lockb0x.Tests;

public class VerifierServiceTests
{
    [Fact]
    public async Task VerifierService_Throws_NotImplemented()
    {
        var service = new VerifierService();
        var entry = new CodexEntry();
        await Assert.ThrowsAsync<NotImplementedException>(() => service.VerifyAsync(entry));
    }
}
