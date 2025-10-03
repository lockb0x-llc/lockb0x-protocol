using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Lockb0x.Core.Models;

namespace Lockb0x.Storage;

/// <summary>
/// Contract implemented by storage backends for the Lockb0x protocol.
/// </summary>
public interface IStorageAdapter
{
    /// <summary>
    /// Stores the supplied payload in the backing store and returns a populated storage descriptor.
    /// </summary>
    Task<StorageResult> StoreAsync(StorageUploadRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the payload identified by <paramref name="location"/> as a read-only stream.
    /// </summary>
    Task<Stream> FetchAsync(string location, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether the supplied location currently resolves to content in the backing store.
    /// </summary>
    Task<bool> ExistsAsync(string location, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves descriptive metadata for the supplied location.
    /// </summary>
    Task<StorageResult> GetMetadataAsync(string location, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the request payload for storing content in an adapter.
/// </summary>
public sealed class StorageUploadRequest
{
    public StorageUploadRequest(Stream content, string fileName, string mediaType)
    {
        Content = content ?? throw new ArgumentNullException(nameof(content));
        if (!content.CanRead)
        {
            throw new ArgumentException("The provided content stream must be readable.", nameof(content));
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("A non-empty file name is required.", nameof(fileName));
        }

        if (string.IsNullOrWhiteSpace(mediaType))
        {
            throw new ArgumentException("A non-empty media type is required.", nameof(mediaType));
        }

        FileName = fileName;
        MediaType = mediaType;
    }

    /// <summary>
    /// The payload stream to upload.
    /// </summary>
    public Stream Content { get; }

    /// <summary>
    /// Logical name of the file being uploaded.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// MIME media type for the payload.
    /// </summary>
    public string MediaType { get; }

    /// <summary>
    /// Optionally supplies a known byte length to optimise upload metadata.
    /// </summary>
    public long? ContentLength { get; init; }
}

/// <summary>
/// Represents the canonical result returned by storage adapters.
/// </summary>
public sealed record StorageResult
{
    public required StorageDescriptor Descriptor { get; init; }

    /// <summary>
    /// The native identifier supplied by the backend (e.g., CID for IPFS).
    /// </summary>
    public required string ContentIdentifier { get; init; }

    /// <summary>
    /// Canonical resource URI that can be dereferenced for audits.
    /// </summary>
    public required string ResourceUri { get; init; }

    /// <summary>
    /// Optional backend-specific metadata returned alongside the descriptor.
    /// </summary>
    public IReadOnlyDictionary<string, string>? NativeMetadata { get; init; }
}

/// <summary>
/// Error raised when a storage adapter encounters an unrecoverable condition.
/// </summary>
public sealed class StorageAdapterException : Exception
{
    public StorageAdapterException(string message)
        : base(message)
    {
    }

    public StorageAdapterException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public class S3StorageAdapter : IStorageAdapter
{
    public Task<StorageResult> StoreAsync(StorageUploadRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<Stream> FetchAsync(string location, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<bool> ExistsAsync(string location, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<StorageResult> GetMetadataAsync(string location, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}

public class GcsStorageAdapter : IStorageAdapter
{
    public Task<StorageResult> StoreAsync(StorageUploadRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<Stream> FetchAsync(string location, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<bool> ExistsAsync(string location, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<StorageResult> GetMetadataAsync(string location, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}

public class LocalStorageAdapter : IStorageAdapter
{
    public Task<StorageResult> StoreAsync(StorageUploadRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<Stream> FetchAsync(string location, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<bool> ExistsAsync(string location, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<StorageResult> GetMetadataAsync(string location, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
