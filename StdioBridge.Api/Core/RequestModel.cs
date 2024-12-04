using System.Text.Json.Serialization;

namespace StdioBridge.Api.Core;

internal class RequestModel
{
    [JsonPropertyName("id")] public required Guid Id { get; init; }
    [JsonPropertyName("url")] public required string Url { get; init; }
    [JsonPropertyName("method")] public required string Method { get; init; }
    [JsonPropertyName("stream")] public bool Stream { get; init; } = false;
}