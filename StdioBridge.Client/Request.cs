using System.Text.Json.Serialization;

namespace StdioBridge.Client;

internal class Request
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("method")] public required string Method { get; init; }
    [JsonPropertyName("url")] public required string Url { get; init; }
    [JsonPropertyName("data")] public object? Data { get; init; }
}