using System;

namespace Lockb0x.Storage;

/// <summary>
/// Configuration for the IPFS storage adapter.
/// </summary>
public sealed class IpfsStorageOptions
{
    /// <summary>
    /// API endpoint for the IPFS node (defaults to the local go-ipfs daemon).
    /// </summary>
    public Uri ApiEndpoint { get; init; } = new("http://127.0.0.1:5001/");

    /// <summary>
    /// Optional public gateway used for retrieval and metadata lookups.
    /// </summary>
    public Uri? GatewayEndpoint { get; init; } = new("https://ipfs.io/");

    /// <summary>
    /// Declared technical region in which content is pinned.
    /// </summary>
    public string Region { get; init; } = "global";

    /// <summary>
    /// Legal jurisdiction governing the storage provider.
    /// </summary>
    public string Jurisdiction { get; init; } = "UN";

    /// <summary>
    /// Name of the service provider responsible for storage custody.
    /// </summary>
    public string Provider { get; init; } = "IPFS";

    /// <summary>
    /// Whether files should be pinned on add.
    /// </summary>
    public bool PinByDefault { get; init; } = true;

    /// <summary>
    /// CID version to request when adding files (defaults to v1 as per spec guidance).
    /// </summary>
    public int CidVersion { get; init; } = 1;

    /// <summary>
    /// Whether to enable raw leaves when uploading (recommended for deterministic hashing).
    /// </summary>
    public bool UseRawLeaves { get; init; } = true;
}
