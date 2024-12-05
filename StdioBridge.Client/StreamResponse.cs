using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StdioBridge.Client;

public class StreamResponse()
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("code")] public int Code { get; set; }
    internal List<string> Buffer { get; set; } = [];
    internal bool Finished { get; set; }
}

internal class StreamChunk<T>()
{
    [JsonPropertyName("id")] public Guid Id { get; init; }
    [JsonPropertyName("data")] public required T Data { get; init; }
}

public class StreamResponse<T> : StreamResponse, IAsyncEnumerable<T>, IEnumerable<T>
{
    internal StreamResponse(StreamResponse stream)
    {
        Id = stream.Id;
        Code = stream.Code;
    }
    
    internal StreamResponse(Guid id, int code)
    {
        Id = id;
        Code = code;
    }
    
    public async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = new())
    {
        while (!Finished && !cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(100, cancellationToken);
            var lst = Buffer;
            Buffer = [];
            foreach (var el in lst)
            {
                var res = JsonSerializer.Deserialize<StreamChunk<T>>(el);
                if (res != null)
                    yield return res.Data;
            }
        }
    }

    public IEnumerator GetEnumerator()
    {
        while (!Finished)
        {
            var lst = Buffer;
            Buffer = [];
            foreach (var el in lst)
            {
                var res = JsonSerializer.Deserialize<T>(el);
                if (res != null)
                    yield return res;
            }
        }
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        while (!Finished)
        {
            var lst = Buffer;
            Buffer = [];
            foreach (var el in lst)
            {
                var res = JsonSerializer.Deserialize<T>(el);
                if (res != null)
                    yield return res;
            }
        }
    }
}