using System.Text.Json.Serialization;

namespace StdioBridge.Client;

public class Response<T>
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("code")] public required int Code { get; init; }
    [JsonPropertyName("data")] public T? Data { get; init; }

    public bool IsSuccessStatusCode => Code < 400;
}