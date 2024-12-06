using System.Diagnostics;
using System.Text.Json;
using StdioBridge.Client.Exceptions;

namespace StdioBridge.Client;

public class BridgeClient
{
    private ProcessStartInfo StartInfo { get; }
    public Process? Process { get; private set; }
    private Dictionary<Guid, string> _responses = [];
    private Dictionary<Guid, StreamResponse> _streams = [];

    private const string ResponseMarker = "!response!";
    private const string StreamStartMarker = "!stream_start!";
    private const string StreamChunkMarker = "!stream_chunk!";
    private const string StreamEndMarker = "!stream_end!";

    public BridgeClient(ProcessStartInfo startInfo)
    {
        StartInfo = startInfo;
        StartProcess();
    }

    private void StartProcess()
    {
        StartInfo.RedirectStandardInput = true;
        StartInfo.RedirectStandardOutput = true;
        StartInfo.UseShellExecute = false;
        var proc = Process.Start(StartInfo);
        if (proc == null)
            return;
        Process = proc;
        ReadOutputLoop();
    }

    private async void ReadOutputLoop()
    {
        var processId = Process?.Id;
        while (Process != null && Process.Id == processId)
        {
            var line = await Process.StandardOutput.ReadLineAsync();
            if (line?.StartsWith(ResponseMarker) == true)
            {
                try
                {
                    var respString = line.AsSpan(ResponseMarker.Length).ToString();
                    var resp = JsonSerializer.Deserialize<BaseResponse>(respString);
                    if (resp != null)
                        _responses[resp.Id] = respString;
                }
                catch (JsonException e)
                {
                }
            }
            else if (line?.StartsWith(StreamStartMarker) == true)
            {
                try
                {
                    var respString = line.AsSpan(StreamStartMarker.Length).ToString();
                    var resp = JsonSerializer.Deserialize<BaseResponse>(respString);
                    if (resp != null)
                        _responses[resp.Id] = respString;
                }
                catch (JsonException e)
                {
                }
            }
            else if (line?.StartsWith(StreamChunkMarker) == true)
            {
                try
                {
                    var respString = line.AsSpan(StreamChunkMarker.Length).ToString();
                    var resp = JsonSerializer.Deserialize<BaseResponse>(respString);
                    if (resp != null)
                    {
                        while (!_streams.ContainsKey(resp.Id))
                        {
                            await Task.Delay(100);
                        }
                        _streams[resp.Id].Buffer.Add(respString);
                    }
                }
                catch (JsonException e)
                {
                }
            }
            else if (line?.StartsWith(StreamEndMarker) == true)
            {
                try
                {
                    var respString = line.AsSpan(StreamEndMarker.Length).ToString();
                    var resp = JsonSerializer.Deserialize<StreamResponse>(respString);
                    if (resp != null)
                    {
                        while (!_streams.ContainsKey(resp.Id))
                        {
                            await Task.Delay(100);
                        }
                        _streams[resp.Id].Code = resp.Code;
                        _streams[resp.Id].Finished = true;
                        _streams.Remove(resp.Id);
                    }
                }
                catch (JsonException e)
                {
                }
            }
            else
            {
                Console.WriteLine(line);
            }
        }
    }

    private async Task<Response> SendRequestAsync(Request request)
    {
        if (Process != null)
        {
            await Process.StandardInput.WriteLineAsync(JsonSerializer.Serialize(request));
            await Process.StandardInput.FlushAsync();
        }

        while (!_responses.ContainsKey(request.Id))
        {
            await Task.Delay(200);
        }

        var source = _responses[request.Id];
        _responses.Remove(request.Id);
        Response? res = null;
        try
        {
            res = JsonSerializer.Deserialize<Response>(source);
        }
        catch (JsonException)
        {
        }

        if (res != null)
        {
            res.Source = source;
            return res;
        }

        return new Response { Code = 400 };
    }

    private async Task<StreamResponse<T>> SendStreamRequestAsync<T>(Request request)
    {
        request.Stream = true;
        if (Process != null)
        {
            await Process.StandardInput.WriteLineAsync(JsonSerializer.Serialize(request));
            await Process.StandardInput.FlushAsync();
        }

        while (!_responses.ContainsKey(request.Id))
        {
            await Task.Delay(200);
        }

        StreamResponse<T>? res = null;
        try
        {
            var stream = JsonSerializer.Deserialize<StreamResponse>(_responses[request.Id]);
            if (stream != null)
                res = new StreamResponse<T>(stream);
            else
                res = new StreamResponse<T>(request.Id, 400);
        }
        catch (JsonException)
        {
        }

        _responses.Remove(request.Id);
        if (res != null)
        {
            _streams.Add(request.Id, res);
            return res;
        }

        return new StreamResponse<T>(request.Id, 400);
    }

    private async Task<T?> SendRequestAsync<T>(Request request)
    {
        var response = await SendRequestAsync(request);
        if (!response.IsSuccessStatusCode)
            throw new BridgeException(response.Code);
        return response.GetData<T>();
    }

    public async Task<Response> GetAsync(string url, object? data = null)
    {
        return await SendRequestAsync(new Request { Id = Guid.NewGuid(), Url = url, Method = "get", Data = data });
    }

    public async Task<Response> PostAsync(string url, object? data)
    {
        return await SendRequestAsync(new Request { Id = Guid.NewGuid(), Url = url, Method = "post", Data = data });
    }

    public async Task<Response> PutAsync(string url, object? data)
    {
        return await SendRequestAsync(new Request { Id = Guid.NewGuid(), Url = url, Method = "put", Data = data });
    }

    public async Task<Response> DeleteAsync(string url, object? data = null)
    {
        return await SendRequestAsync(new Request
            { Id = Guid.NewGuid(), Url = url, Method = "delete", Data = data });
    }

    public async Task<T?> GetAsync<T>(string url, object? data = null)
    {
        return await SendRequestAsync<T>(new Request { Id = Guid.NewGuid(), Url = url, Method = "get", Data = data });
    }

    public async Task<T?> PostAsync<T>(string url, object? data)
    {
        return await SendRequestAsync<T>(new Request { Id = Guid.NewGuid(), Url = url, Method = "post", Data = data });
    }

    public async Task<T?> PutAsync<T>(string url, object? data)
    {
        return await SendRequestAsync<T>(new Request { Id = Guid.NewGuid(), Url = url, Method = "put", Data = data });
    }

    public async Task<T?> DeleteAsync<T>(string url, object? data = null)
    {
        return await SendRequestAsync<T>(new Request { Id = Guid.NewGuid(), Url = url, Method = "delete", Data = data });
    }

    public async Task<StreamResponse<T>> GetStreamAsync<T>(string url, object? data = null)
    {
        return await SendStreamRequestAsync<T>(new Request
            { Id = Guid.NewGuid(), Url = url, Method = "get", Data = data });
    }

    public async Task<StreamResponse<T>> PostStreamAsync<T>(string url, object? data = null)
    {
        return await SendStreamRequestAsync<T>(new Request
            { Id = Guid.NewGuid(), Url = url, Method = "post", Data = data });
    }

    public async Task<StreamResponse<T>> PutStreamAsync<T>(string url, object? data = null)
    {
        return await SendStreamRequestAsync<T>(new Request
            { Id = Guid.NewGuid(), Url = url, Method = "put", Data = data });
    }

    public async Task<StreamResponse<T>> DeleteStreamAsync<T>(string url, object? data = null)
    {
        return await SendStreamRequestAsync<T>(new Request
            { Id = Guid.NewGuid(), Url = url, Method = "delete", Data = data });
    }
}