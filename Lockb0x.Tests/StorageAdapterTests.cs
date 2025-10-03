using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lockb0x.Core.Utilities;
using Lockb0x.Storage;
using Lockb0x.Storage.Adapters;
using Xunit;

namespace Lockb0x.Tests;

public class StorageAdapterTests
{
    [Fact]
    public async Task StoreAsync_ComputesIntegrityProofAndMetadata()
    {
        var payload = Encoding.UTF8.GetBytes("hello world");
        var sha = SHA256.HashData(payload);
        var cid = CreateRawCid(sha);

        var apiHandler = new StubHttpMessageHandler(async (request, token) =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Contains("api/v0/add", request.RequestUri!.AbsoluteUri, StringComparison.Ordinal);
            var body = await request.Content!.ReadAsStringAsync(token).ConfigureAwait(false);
            Assert.Contains("hello world", body, StringComparison.Ordinal);

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($"{{\"Name\":\"hello.txt\",\"Hash\":\"{cid}\",\"Size\":\"11\"}}\n", Encoding.UTF8, "application/json")
            };
            return response;
        });

        var options = new IpfsStorageOptions
        {
            ApiEndpoint = new Uri("http://localhost:5001/"),
            GatewayEndpoint = null,
            Region = "us-central1",
            Jurisdiction = "US/CA",
            Provider = "IPFS Cooperative"
        };

        await using var adapter = new IpfsStorageAdapter(options, new HttpClient(apiHandler) { BaseAddress = options.ApiEndpoint }, gatewayClient: null);
        using var stream = new MemoryStream(payload);
        var request = new StorageUploadRequest(stream, "hello.txt", "text/plain") { ContentLength = payload.Length };

        var result = await adapter.StoreAsync(request, CancellationToken.None);

        Assert.Equal("ipfs", result.Descriptor.Protocol);
        Assert.Equal(NiUri.Create(sha, "sha-256"), result.Descriptor.IntegrityProof);
        Assert.Equal("text/plain", result.Descriptor.MediaType);
        Assert.Equal(payload.Length, result.Descriptor.SizeBytes);
        Assert.Equal("us-central1", result.Descriptor.Location.Region);
        Assert.Equal("US/CA", result.Descriptor.Location.Jurisdiction);
        Assert.Equal("IPFS Cooperative", result.Descriptor.Location.Provider);
        Assert.Equal(cid, result.ContentIdentifier);
        Assert.Equal($"ipfs://{cid}", result.ResourceUri);
        Assert.NotNull(result.NativeMetadata);
        var nativeMetadata = result.NativeMetadata;
        Assert.Contains("Name", nativeMetadata.Keys);
    }

    [Fact]
    public async Task StoreAsync_ThrowsWhenCidDigestMismatch()
    {
        var payload = Encoding.UTF8.GetBytes("mismatch");
        var incorrectDigest = new byte[32];
        var mismatchedCid = CreateRawCid(incorrectDigest);

        var apiHandler = new StubHttpMessageHandler(async (request, token) =>
        {
            await request.Content!.CopyToAsync(Stream.Null, token).ConfigureAwait(false);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent($"{{\"Name\":\"doc.txt\",\"Hash\":\"{mismatchedCid}\",\"Size\":\"8\"}}\n", Encoding.UTF8, "application/json")
            };
            return response;
        });

        var options = new IpfsStorageOptions
        {
            ApiEndpoint = new Uri("http://localhost:5001/"),
            GatewayEndpoint = null,
            Region = "global",
            Jurisdiction = "UN",
            Provider = "IPFS"
        };

        await using var adapter = new IpfsStorageAdapter(options, new HttpClient(apiHandler) { BaseAddress = options.ApiEndpoint }, gatewayClient: null);
        using var stream = new MemoryStream(payload);
        var request = new StorageUploadRequest(stream, "doc.txt", "text/plain");

        await Assert.ThrowsAsync<StorageAdapterException>(() => adapter.StoreAsync(request, CancellationToken.None));
    }

    [Fact]
    public async Task GetMetadataAsync_UsesGatewayHeadersWhenAvailable()
    {
        var payload = Encoding.UTF8.GetBytes("gateway");
        var digest = SHA256.HashData(payload);
        var cid = CreateRawCid(digest);

        var apiHandler = new StubHttpMessageHandler((request, token) =>
        {
            if (request.RequestUri!.AbsolutePath.Contains("block/stat", StringComparison.Ordinal))
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"Size\":7}\n", Encoding.UTF8, "application/json")
                };
                return Task.FromResult(response);
            }

            throw new InvalidOperationException($"Unexpected API call: {request.RequestUri}");
        });

        var gatewayHandler = new StubHttpMessageHandler((request, token) =>
        {
            if (request.Method == HttpMethod.Head)
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(string.Empty)
                };
                response.Content.Headers.ContentLength = payload.Length;
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                return Task.FromResult(response);
            }

            throw new InvalidOperationException($"Unexpected gateway call: {request.Method} {request.RequestUri}");
        });

        var options = new IpfsStorageOptions
        {
            ApiEndpoint = new Uri("http://localhost:5001/"),
            GatewayEndpoint = new Uri("https://gateway.example/"),
            Region = "eu-west-1",
            Jurisdiction = "EU/DE",
            Provider = "Gateway Provider"
        };

        await using var adapter = new IpfsStorageAdapter(
            options,
            new HttpClient(apiHandler) { BaseAddress = options.ApiEndpoint },
            new HttpClient(gatewayHandler) { BaseAddress = options.GatewayEndpoint });

        var result = await adapter.GetMetadataAsync(cid, CancellationToken.None);

        Assert.Equal("ipfs", result.Descriptor.Protocol);
        Assert.Equal(NiUri.Create(digest, "sha-256"), result.Descriptor.IntegrityProof);
        Assert.Equal(payload.Length, result.Descriptor.SizeBytes);
        Assert.Equal("text/plain", result.Descriptor.MediaType);
        Assert.Equal("https://gateway.example/ipfs/" + cid, result.ResourceUri);
        Assert.Equal("Gateway Provider", result.Descriptor.Location.Provider);
        Assert.Equal("eu-west-1", result.Descriptor.Location.Region);
        Assert.Equal("EU/DE", result.Descriptor.Location.Jurisdiction);
        Assert.Equal(cid, result.ContentIdentifier);
    }

    private static string CreateRawCid(ReadOnlySpan<byte> digest)
    {
        if (digest.Length != 32)
        {
            throw new ArgumentException("SHA-256 digest must be 32 bytes.", nameof(digest));
        }

        Span<byte> payload = stackalloc byte[4 + digest.Length];
        payload[0] = 0x01; // CIDv1
        payload[1] = 0x55; // raw binary codec
        payload[2] = 0x12; // sha2-256 multihash code
        payload[3] = (byte)digest.Length;
        digest.CopyTo(payload[4..]);

        var encoded = EncodeBase32(payload);
        return "b" + encoded;
    }

    private static string EncodeBase32(ReadOnlySpan<byte> data)
    {
        const string alphabet = "abcdefghijklmnopqrstuvwxyz234567";
        if (data.IsEmpty)
        {
            return string.Empty;
        }

        var builder = new StringBuilder((data.Length * 8 + 4) / 5);
        int buffer = 0;
        int bits = 0;
        foreach (var b in data)
        {
            buffer = (buffer << 8) | b;
            bits += 8;
            while (bits >= 5)
            {
                bits -= 5;
                builder.Append(alphabet[(buffer >> bits) & 0x1F]);
            }
        }

        if (bits > 0)
        {
            builder.Append(alphabet[(buffer << (5 - bits)) & 0x1F]);
        }

        return builder.ToString();
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

        public StubHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => _handler(request, cancellationToken);
    }
}
