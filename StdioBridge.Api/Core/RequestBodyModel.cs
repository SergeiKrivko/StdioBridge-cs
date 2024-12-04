using System.Text.Json.Serialization;

namespace StdioBridge.Api.Core;

internal class RequestBodyModel<T>
{
    [JsonPropertyName("data")] public required T Data { get; init; }
}