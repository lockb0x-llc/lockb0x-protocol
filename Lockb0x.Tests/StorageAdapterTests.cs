using Lockb0x.Storage;
using Xunit;
using System;
using System.Threading.Tasks;

namespace Lockb0x.Tests;

public class StorageAdapterTests
{
    [Fact]
    public async Task AllAdapters_Throw_NotImplemented()
    {
        IStorageAdapter[] adapters = new IStorageAdapter[]
        {
            new IpfsStorageAdapter(),
            new S3StorageAdapter(),
            new GcsStorageAdapter(),
            new LocalStorageAdapter()
        };
        foreach (var adapter in adapters)
        {
            await Assert.ThrowsAsync<NotImplementedException>(() => adapter.StoreAsync(new byte[] { 1 }, "file"));
            await Assert.ThrowsAsync<NotImplementedException>(() => adapter.FetchAsync("loc"));
            await Assert.ThrowsAsync<NotImplementedException>(() => adapter.ExistsAsync("loc"));
            await Assert.ThrowsAsync<NotImplementedException>(() => adapter.GetMetadataAsync("loc"));
        }
    }
}
