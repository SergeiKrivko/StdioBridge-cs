using System.Text.Json.Serialization;

namespace StdioBridge.Client;

internal class BaseResponse
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
}