using System.Text.Json;
using System.Text.Json.Serialization;

namespace StdioBridge.Client;

public class Response
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("code")] public required int Code { get; init; }
    [JsonPropertyName("data")] public object? Data { get; init; }
    
    [JsonIgnore] internal string? Source { get; set; }

    public T? GetData<T>()
    {
        if (Source == null)
            return default;
        try
        {
            var r = JsonSerializer.Deserialize<DataResponse<T>>(Source);
            return r == null ? default : r.Data;
        }
        catch (Exception)
        {
            return default;
        }
    }

    public bool IsSuccessStatusCode => Code < 400;
}

internal class DataResponse<T>
{
    [JsonPropertyName("data")] public T? Data { get; init; }
}