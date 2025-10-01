using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;

namespace Lockb0x.Core.Canonicalization;

/// <summary>
/// Implements RFC 8785 JSON Canonicalization Scheme (JCS) using System.Text.Json primitives.
/// The implementation preserves deterministic ordering, string escaping, and numeric formatting
/// required for signature interoperability.
/// </summary>
public sealed class JcsCanonicalizer : IJsonCanonicalizer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.General)
    {
        PropertyNamingPolicy = null,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public string Canonicalize<T>(T payload)
    {
        using var document = JsonSerializer.SerializeToDocument(payload, SerializerOptions);
        using var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer, new JsonWriterOptions
               {
                   Indented = false,
                   SkipValidation = false
               }))
        {
            WriteCanonicalElement(document.RootElement, writer);
        }

        return Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    public byte[] Hash<T>(T payload, HashAlgorithmName algorithm)
    {
        var canonical = Canonicalize(payload);
        using var hash = IncrementalHash.CreateHash(algorithm);
        hash.AppendData(Encoding.UTF8.GetBytes(canonical));
        return hash.GetHashAndReset();
    }

    private static void WriteCanonicalElement(JsonElement element, Utf8JsonWriter writer)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in element.EnumerateObject().OrderBy(p => p.Name, StringComparer.Ordinal))
                {
                    writer.WritePropertyName(property.Name);
                    WriteCanonicalElement(property.Value, writer);
                }
                writer.WriteEndObject();
                break;
            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray())
                {
                    WriteCanonicalElement(item, writer);
                }
                writer.WriteEndArray();
                break;
            case JsonValueKind.String:
                writer.WriteStringValue(element.GetString());
                break;
            case JsonValueKind.Number:
                if (element.TryGetInt64(out var longValue))
                {
                    writer.WriteNumberValue(longValue);
                }
                else if (element.TryGetDecimal(out var decimalValue))
                {
                    writer.WriteNumberValue(decimalValue);
                }
                else
                {
                    writer.WriteNumberValue(element.GetDouble());
                }
                break;
            case JsonValueKind.True:
                writer.WriteBooleanValue(true);
                break;
            case JsonValueKind.False:
                writer.WriteBooleanValue(false);
                break;
            case JsonValueKind.Null:
                writer.WriteNullValue();
                break;
            default:
                throw new NotSupportedException($"Unsupported JSON value kind '{element.ValueKind}'.");
        }
    }
}
