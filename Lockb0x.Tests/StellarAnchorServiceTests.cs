using Lockb0x.Anchor.Stellar;
using Lockb0x.Core;
using Xunit;
using System;
using System.Threading.Tasks;

namespace Lockb0x.Tests;

public class StellarAnchorServiceTests
{
    [Fact]
    public async Task StellarAnchorService_Throws_NotImplemented()
    {
        var service = new StellarAnchorService();
        var entry = new CodexEntry();
        var anchor = new AnchorProof();
        await Assert.ThrowsAsync<NotImplementedException>(() => service.AnchorAsync(entry));
        await Assert.ThrowsAsync<NotImplementedException>(() => service.VerifyAnchorAsync(anchor, entry));
        await Assert.ThrowsAsync<NotImplementedException>(() => service.GetTransactionUrlAsync(anchor));
    }
}
