using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Lockb0x.Anchor.Stellar;

/// <summary>
/// Horizon client backed by HTTP requests to a Stellar node.
/// </summary>
public sealed class HttpStellarHorizonClient : IStellarHorizonClient
{
    private readonly HttpClient _httpClient;

    public HttpStellarHorizonClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<StellarTransactionRecord?> FindTransactionByMemoAsync(StellarMemo memo, StellarNetworkOptions network, CancellationToken cancellationToken)
    {
        if (memo is null)
        {
            throw new ArgumentNullException(nameof(memo));
        }

        var uri = BuildUri(network.HorizonEndpoint, $"transactions?memo={Uri.EscapeDataString(memo.ToBase64())}&limit=1&order=desc");
        using var response = await _httpClient.GetAsync(uri, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (!document.RootElement.TryGetProperty("_embedded", out var embedded) ||
            !embedded.TryGetProperty("records", out var records) ||
            records.GetArrayLength() == 0)
        {
            return null;
        }

        return ParseTransaction(records[0]);
    }

    public async Task<StellarTransactionRecord?> GetTransactionAsync(string transactionHash, StellarNetworkOptions network, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(transactionHash))
        {
            throw new ArgumentException("Transaction hash must be provided.", nameof(transactionHash));
        }

        var uri = BuildUri(network.HorizonEndpoint, $"transactions/{transactionHash}");
        using var response = await _httpClient.GetAsync(uri, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
        return ParseTransaction(document.RootElement);
    }

    public async Task<StellarTransactionRecord> SubmitTransactionAsync(StellarSubmitTransactionRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.EnvelopeXdr))
        {
            throw new StellarAnchorException("anchor.envelope_missing", "A signed transaction envelope (base64 XDR) is required to submit to Horizon.");
        }

        var uri = BuildUri(request.Network.HorizonEndpoint, "transactions");
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["tx"] = request.EnvelopeXdr!
        });

        using var response = await _httpClient.PostAsync(uri, content, cancellationToken).ConfigureAwait(false);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new StellarAnchorException("anchor.horizon_error", $"Horizon rejected the transaction: {(int)response.StatusCode} {payload}");
        }

        using var document = JsonDocument.Parse(payload);
        if (!document.RootElement.TryGetProperty("hash", out var hashProperty) || hashProperty.ValueKind != JsonValueKind.String)
        {
            throw new StellarAnchorException("anchor.horizon_unexpected", "Horizon response did not contain a transaction hash.");
        }

        var hash = hashProperty.GetString()!;
        var record = await GetTransactionAsync(hash, request.Network, cancellationToken).ConfigureAwait(false);
        if (record is null)
        {
            // Fall back to minimal record using memo bytes when Horizon does not return the transaction immediately.
            return new StellarTransactionRecord(hash, request.Memo.Value.ToArray(), request.Memo.MemoType, request.RequestedAt ?? DateTimeOffset.UtcNow, request.PublicKey);
        }

        return record;
    }

    private static Uri BuildUri(Uri baseUri, string relativePath)
    {
        if (!baseUri.AbsoluteUri.EndsWith('/'))
        {
            baseUri = new Uri(baseUri.AbsoluteUri + '/');
        }

        return new Uri(baseUri, relativePath);
    }

    private static StellarTransactionRecord? ParseTransaction(JsonElement element)
    {
        if (!element.TryGetProperty("hash", out var hashElement) || hashElement.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var memoType = element.TryGetProperty("memo_type", out var memoTypeElement) && memoTypeElement.ValueKind == JsonValueKind.String
            ? memoTypeElement.GetString() ?? "hash"
            : "hash";

        byte[] memoBytes = Array.Empty<byte>();
        if (element.TryGetProperty("memo", out var memoElement) && memoElement.ValueKind == JsonValueKind.String)
        {
            var memoValue = memoElement.GetString();
            if (!string.IsNullOrEmpty(memoValue))
            {
                memoBytes = memoType switch
                {
                    "text" => Encoding.UTF8.GetBytes(memoValue),
                    _ => Convert.FromBase64String(memoValue)
                };
            }
        }

        DateTimeOffset? ledgerCloseTime = null;
        if (element.TryGetProperty("ledger_close_time", out var timeElement) && timeElement.ValueKind == JsonValueKind.String)
        {
            if (DateTimeOffset.TryParse(timeElement.GetString(), out var parsed))
            {
                ledgerCloseTime = parsed;
            }
        }

        var sourceAccount = element.TryGetProperty("source_account", out var accountElement) && accountElement.ValueKind == JsonValueKind.String
            ? accountElement.GetString()
            : null;

        return new StellarTransactionRecord(hashElement.GetString()!, memoBytes, memoType, ledgerCloseTime, sourceAccount);
    }
}
