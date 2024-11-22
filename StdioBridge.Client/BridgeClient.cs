﻿using System.Diagnostics;
using System.Text.Json;

namespace StdioBridge.Client;

public class BridgeClient
{
    public ProcessStartInfo StartInfo { get; }
    public Process? Process { get; private set; }
    private Dictionary<Guid, string> _responses = [];

    private const string ResponseMarker = "!response!";

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
        }
    }

    private async Task<Response<T>> SendRequestAsync<T>(Request request)
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

        Response<T>? res = null;
        try
        {
            res = JsonSerializer.Deserialize<Response<T>>(_responses[request.Id]);
        }
        catch (JsonException)
        {
        }
        
        _responses.Remove(request.Id);
        if (res != null)
        {
            return res;
        }

        return new Response<T> { Code = 400, Data = default };
    }

    public async Task<Response<T>> GetAsync<T>(string url, object? data = null)
    {
        return await SendRequestAsync<T>(new Request { Id = Guid.NewGuid(), Url = url, Method = "get", Data = data });
    }

    public async Task<Response<T>> PostAsync<T>(string url, object? data)
    {
        return await SendRequestAsync<T>(new Request { Id = Guid.NewGuid(), Url = url, Method = "post", Data = data });
    }

    public async Task<Response<T>> PutAsync<T>(string url, object? data)
    {
        return await SendRequestAsync<T>(new Request { Id = Guid.NewGuid(), Url = url, Method = "put", Data = data });
    }

    public async Task<Response<T>> DeleteAsync<T>(string url, object? data = null)
    {
        return await SendRequestAsync<T>(new Request { Id = Guid.NewGuid(), Url = url, Method = "delete", Data = data });
    }
}