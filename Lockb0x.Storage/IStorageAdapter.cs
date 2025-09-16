using System.Threading.Tasks;
using Lockb0x.Core;

namespace Lockb0x.Storage;

public interface IStorageAdapter
{
    Task<StorageProof> StoreAsync(byte[] data, string fileName);
    Task<byte[]> FetchAsync(string location);
    Task<bool> ExistsAsync(string location);
    Task<StorageProof> GetMetadataAsync(string location);
}

// IPFS Adapter stub
public class IpfsStorageAdapter : IStorageAdapter
{
    public Task<StorageProof> StoreAsync(byte[] data, string fileName) => throw new System.NotImplementedException();
    public Task<byte[]> FetchAsync(string location) => throw new System.NotImplementedException();
    public Task<bool> ExistsAsync(string location) => throw new System.NotImplementedException();
    public Task<StorageProof> GetMetadataAsync(string location) => throw new System.NotImplementedException();
}

// S3 Adapter stub
public class S3StorageAdapter : IStorageAdapter
{
    public Task<StorageProof> StoreAsync(byte[] data, string fileName) => throw new System.NotImplementedException();
    public Task<byte[]> FetchAsync(string location) => throw new System.NotImplementedException();
    public Task<bool> ExistsAsync(string location) => throw new System.NotImplementedException();
    public Task<StorageProof> GetMetadataAsync(string location) => throw new System.NotImplementedException();
}

// GCS Adapter stub
public class GcsStorageAdapter : IStorageAdapter
{
    public Task<StorageProof> StoreAsync(byte[] data, string fileName) => throw new System.NotImplementedException();
    public Task<byte[]> FetchAsync(string location) => throw new System.NotImplementedException();
    public Task<bool> ExistsAsync(string location) => throw new System.NotImplementedException();
    public Task<StorageProof> GetMetadataAsync(string location) => throw new System.NotImplementedException();
}

// Mock/Local Adapter stub
public class LocalStorageAdapter : IStorageAdapter
{
    public Task<StorageProof> StoreAsync(byte[] data, string fileName) => throw new System.NotImplementedException();
    public Task<byte[]> FetchAsync(string location) => throw new System.NotImplementedException();
    public Task<bool> ExistsAsync(string location) => throw new System.NotImplementedException();
    public Task<StorageProof> GetMetadataAsync(string location) => throw new System.NotImplementedException();
}
