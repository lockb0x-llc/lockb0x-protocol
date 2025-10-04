using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Lockb0x.Core.Models;
using Lockb0x.Core.Utilities;
using Lockb0x.Storage.Internal;

namespace Lockb0x.Storage.Adapters;

/// <summary>
/// Storage adapter that integrates with an IPFS node via the HTTP API.
/// </summary>
public sealed class IpfsStorageAdapter : IStorageAdapter, IAsyncDisposable
{
    private readonly HttpClient _apiClient;
    private readonly HttpClient? _gatewayClient;
    private readonly bool _disposeApiClient;
    private readonly bool _disposeGatewayClient;
    private readonly IpfsStorageOptions _options;

    public IpfsStorageAdapter(IpfsStorageOptions options)
        : this(options, apiClient: null, gatewayClient: null)
    {
    }

    public IpfsStorageAdapter(IpfsStorageOptions options, HttpClient? apiClient, HttpClient? gatewayClient)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        if (options.ApiEndpoint is null)
        {
            throw new ArgumentException("An IPFS API endpoint must be supplied.", nameof(options));
        }

        if (string.IsNullOrWhiteSpace(options.Region))
        {
            throw new ArgumentException("Region must be provided for IPFS storage metadata.", nameof(options));
        }

        if (string.IsNullOrWhiteSpace(options.Jurisdiction))
        {
            throw new ArgumentException("Jurisdiction must be provided for IPFS storage metadata.", nameof(options));
        }

        if (string.IsNullOrWhiteSpace(options.Provider))
        {
            throw new ArgumentException("Provider must be provided for IPFS storage metadata.", nameof(options));
        }

        _apiClient = apiClient ?? CreateHttpClient(NormalizeBaseAddress(options.ApiEndpoint));
        _disposeApiClient = apiClient is null;

        if (gatewayClient is not null)
        {
            _gatewayClient = gatewayClient;
        }
        else if (options.GatewayEndpoint is not null)
        {
            _gatewayClient = CreateHttpClient(NormalizeBaseAddress(options.GatewayEndpoint));
            _disposeGatewayClient = true;
        }
    }

    public async Task<StorageResult> StoreAsync(StorageUploadRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (cid, size, digest, nativeMetadata) = await AddToIpfsAsync(request, cancellationToken).ConfigureAwait(false);
        var cidInfo = CidUtility.Parse(cid);
        if (cidInfo.UsesSha256)
        {
            if (!digest.AsSpan().SequenceEqual(cidInfo.Digest))
            {
                throw new StorageAdapterException("CID digest mismatch detected after IPFS upload.");
            }
        }

        var integrityDigest = cidInfo.UsesSha256 ? cidInfo.Digest : digest;
        var descriptor = new StorageDescriptor
        {
            Protocol = "ipfs",
            IntegrityProof = NiUri.Create(integrityDigest, "sha-256"),
            MediaType = request.MediaType,
            SizeBytes = size,
            Location = BuildLocation()
        };

        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Name"] = nativeMetadata.Name,
            ["Pin"] = _options.PinByDefault ? "true" : "false",
            ["CidVersion"] = cidInfo.Version.ToString(CultureInfo.InvariantCulture)
        };
        if (!string.IsNullOrEmpty(nativeMetadata.SizeFromApi))
        {
            metadata["ReportedSize"] = nativeMetadata.SizeFromApi!;
        }

        return new StorageResult
        {
            Descriptor = descriptor,
            ContentIdentifier = cid,
            ResourceUri = BuildResourceUri(cid),
            NativeMetadata = metadata
        };
    }

    public async Task<Stream> FetchAsync(string location, CancellationToken cancellationToken = default)
    {
        var cid = NormalizeLocation(location);
        return await FetchContentStreamInternalAsync(cid, cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> ExistsAsync(string location, CancellationToken cancellationToken = default)
    {
        var cid = NormalizeLocation(location);
        try
        {
            if (_gatewayClient is not null)
            {
                using var headRequest = new HttpRequestMessage(HttpMethod.Head, $"ipfs/{cid}");
                using var headResponse = await _gatewayClient.SendAsync(headRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                if (headResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    return false;
                }

                if (headResponse.StatusCode == HttpStatusCode.MethodNotAllowed)
                {
                    using var rangeRequest = new HttpRequestMessage(HttpMethod.Get, $"ipfs/{cid}");
                    rangeRequest.Headers.Range = new RangeHeaderValue(0, 0);
                    using var rangeResponse = await _gatewayClient.SendAsync(rangeRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    if (rangeResponse.StatusCode == HttpStatusCode.NotFound)
                    {
                        return false;
                    }

                    return rangeResponse.IsSuccessStatusCode;
                }

                return headResponse.IsSuccessStatusCode;
            }

            using var response = await _apiClient.PostAsync($"api/v0/block/stat?arg={Uri.EscapeDataString(cid)}", null, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            throw new StorageAdapterException("IPFS availability check failed due to a network error.", ex);
        }
    }

    public async Task<StorageResult> GetMetadataAsync(string location, CancellationToken cancellationToken = default)
    {
        var cid = NormalizeLocation(location);
        var cidInfo = CidUtility.Parse(cid);

        long size = 0;
        string? mediaType = null;
        if (_gatewayClient is not null)
        {
            using var headRequest = new HttpRequestMessage(HttpMethod.Head, $"ipfs/{cid}");
            using var headResponse = await _gatewayClient.SendAsync(headRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            if (headResponse.StatusCode == HttpStatusCode.NotFound)
            {
                throw new StorageAdapterException($"IPFS content '{cid}' was not found at the configured gateway.");
            }

            if (headResponse.IsSuccessStatusCode)
            {
                if (headResponse.Content.Headers.ContentLength is long contentLength)
                {
                    size = contentLength;
                }

                if (headResponse.Content.Headers.ContentType is { } contentType)
                {
                    mediaType = contentType.ToString();
                }
            }
        }

        if (size == 0)
        {
            using var statResponse = await _apiClient.PostAsync($"api/v0/block/stat?arg={Uri.EscapeDataString(cid)}", null, cancellationToken).ConfigureAwait(false);
            if (statResponse.StatusCode == HttpStatusCode.NotFound)
            {
                throw new StorageAdapterException($"IPFS content '{cid}' was not found.");
            }

            statResponse.EnsureSuccessStatusCode();
            using var statDoc = await JsonDocument.ParseAsync(await statResponse.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false), cancellationToken: cancellationToken).ConfigureAwait(false);
            if (statDoc.RootElement.TryGetProperty("Size", out var sizeElement) && sizeElement.ValueKind == JsonValueKind.Number)
            {
                size = sizeElement.GetInt64();
            }
            else if (statDoc.RootElement.TryGetProperty("CumulativeSize", out var cumulative) && cumulative.ValueKind == JsonValueKind.Number)
            {
                size = cumulative.GetInt64();
            }
        }

        byte[] integrityDigest;
        if (cidInfo.UsesSha256)
        {
            integrityDigest = cidInfo.Digest;
        }
        else
        {
            var download = await DownloadAndHashAsync(cid, cancellationToken).ConfigureAwait(false);
            integrityDigest = download.Digest;
            if (download.Size > 0)
            {
                size = download.Size;
            }
            mediaType ??= "application/octet-stream";
        }

        mediaType ??= "application/octet-stream";

        var descriptor = new StorageDescriptor
        {
            Protocol = "ipfs",
            IntegrityProof = NiUri.Create(integrityDigest, "sha-256"),
            MediaType = mediaType,
            SizeBytes = size,
            Location = BuildLocation()
        };

        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["CidVersion"] = cidInfo.Version.ToString(CultureInfo.InvariantCulture),
            ["Codec"] = cidInfo.Codec.ToString(CultureInfo.InvariantCulture)
        };

        return new StorageResult
        {
            Descriptor = descriptor,
            ContentIdentifier = cid,
            ResourceUri = BuildResourceUri(cid),
            NativeMetadata = metadata
        };
    }

    public ValueTask DisposeAsync()
    {
        if (_disposeApiClient)
        {
            _apiClient.Dispose();
        }

        if (_disposeGatewayClient && _gatewayClient is not null)
        {
            _gatewayClient.Dispose();
        }

        return ValueTask.CompletedTask;
    }

    private async Task<(string Cid, long Size, byte[] Digest, (string Name, string? SizeFromApi) NativeMetadata)> AddToIpfsAsync(StorageUploadRequest request, CancellationToken cancellationToken)
    {
        using var hashingStream = new HashingStream(request.Content, leaveOpen: true);
        using var streamContent = new StreamContent(hashingStream);
        streamContent.Headers.ContentType = MediaTypeHeaderValue.Parse(request.MediaType);
        if (request.ContentLength.HasValue)
        {
            streamContent.Headers.ContentLength = request.ContentLength.Value;
        }

        using var form = new MultipartFormDataContent
        {
            { streamContent, "file", request.FileName }
        };

        var endpoint = new StringBuilder("api/v0/add?")
            .Append("pin=").Append(_options.PinByDefault ? "true" : "false")
            .Append("&cid-version=").Append(_options.CidVersion.ToString(CultureInfo.InvariantCulture))
            .Append("&hash=sha2-256")
            .Append("&raw-leaves=").Append(_options.UseRawLeaves ? "true" : "false")
            .Append("&wrap-with-directory=false")
            .Append("&progress=false")
            .ToString();

        using var response = await _apiClient.PostAsync(endpoint, form, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var addResult = await ParseAddResponseAsync(response.Content, cancellationToken).ConfigureAwait(false);
        var digest = hashingStream.GetHashAndReset();
        var size = request.ContentLength ?? hashingStream.BytesProcessed;
        return (addResult.Hash, size, digest, (Name: addResult.Name, SizeFromApi: addResult.Size));
    }

    private static async Task<IpfsAddResponse> ParseAddResponseAsync(HttpContent content, CancellationToken cancellationToken)
    {
        await using var responseStream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var reader = new StreamReader(responseStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
        string? line = null;
        while (!reader.EndOfStream)
        {
            var current = await reader.ReadLineAsync().ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(current))
            {
                line = current;
            }
        }

        if (string.IsNullOrWhiteSpace(line))
        {
            throw new StorageAdapterException("Unexpected empty response from IPFS add operation.");
        }

        using var document = JsonDocument.Parse(line);
        var root = document.RootElement;
        var hash = root.GetProperty("Hash").GetString();
        if (string.IsNullOrWhiteSpace(hash))
        {
            throw new StorageAdapterException("IPFS add response did not include a CID.");
        }

        var size = root.TryGetProperty("Size", out var sizeElement) ? sizeElement.GetString() : null;
        var name = root.TryGetProperty("Name", out var nameElement) ? nameElement.GetString() ?? string.Empty : string.Empty;
        return new IpfsAddResponse(name, hash, size);
    }

    private StorageLocation BuildLocation() => new()
    {
        Region = _options.Region,
        Jurisdiction = _options.Jurisdiction,
        Provider = _options.Provider
    };

    private static Uri NormalizeBaseAddress(Uri uri)
    {
        if (!uri.OriginalString.EndsWith("/", StringComparison.Ordinal))
        {
            return new Uri(uri.OriginalString + "/", UriKind.Absolute);
        }

        return uri;
    }

    private static HttpClient CreateHttpClient(Uri baseAddress)
    {
        var client = new HttpClient
        {
            BaseAddress = baseAddress
        };
        return client;
    }

    private static string NormalizeLocation(string location)
    {
        if (string.IsNullOrWhiteSpace(location))
        {
            throw new ArgumentException("Location cannot be null or empty.", nameof(location));
        }

        var trimmed = location.Trim();
        if (trimmed.StartsWith("ipfs://", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed[7..];
        }

        if (trimmed.StartsWith("/ipfs/", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed[6..];
        }

        var slashIndex = trimmed.IndexOf('/', StringComparison.Ordinal);
        if (slashIndex > 0)
        {
            trimmed = trimmed[..slashIndex];
        }

        return trimmed;
    }

    private string BuildResourceUri(string cid)
        => _gatewayClient is not null
            ? new Uri(_gatewayClient.BaseAddress!, $"ipfs/{cid}").ToString()
            : $"ipfs://{cid}";

    private async Task<(byte[] Digest, long Size)> DownloadAndHashAsync(string cid, CancellationToken cancellationToken)
    {
        await using var responseStream = await FetchContentStreamInternalAsync(cid, cancellationToken).ConfigureAwait(false);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = ArrayPool<byte>.Shared.Rent(81920);
        long total = 0;
        try
        {
            int read;
            while ((read = await responseStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false)) > 0)
            {
                hash.AppendData(buffer.AsSpan(0, read));
                total += read;
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        return (Digest: hash.GetHashAndReset(), Size: total);
    }

    private async Task<HttpResponseStream> FetchContentStreamInternalAsync(string cid, CancellationToken cancellationToken)
    {
        if (_gatewayClient is not null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"ipfs/{cid}");
            var response = await _gatewayClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                response.Dispose();
                throw new StorageAdapterException($"IPFS content '{cid}' was not found at the configured gateway.");
            }

            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            return new HttpResponseStream(stream, response);
        }
        else
        {
            var response = await _apiClient.PostAsync($"api/v0/cat?arg={Uri.EscapeDataString(cid)}", null, cancellationToken).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                response.Dispose();
                throw new StorageAdapterException($"IPFS content '{cid}' was not found.");
            }

            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            return new HttpResponseStream(stream, response);
        }
    }

    private readonly struct IpfsAddResponse
    {
        public IpfsAddResponse(string name, string hash, string? size)
        {
            Name = name;
            Hash = hash;
            Size = size;
        }

        public string Name { get; }
        public string Hash { get; }
        public string? Size { get; }
    }
}
