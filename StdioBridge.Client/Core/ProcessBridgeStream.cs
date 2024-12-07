using System.Diagnostics;

namespace StdioBridge.Client.Core;

internal class ProcessBridgeStream : IBrideClientStream
{
    private ProcessStartInfo StartInfo { get; }
    private Process? Process { get; set; }

    public ProcessBridgeStream(ProcessStartInfo startInfo)
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
            if (line != null)
                OnNewLine?.Invoke(line);
        }
    }

    public event Action<string>? OnNewLine;
    
    public async Task WriteLineAsync(string line)
    {
        if (Process == null)
            return;
        await Process.StandardInput.WriteLineAsync(line);
        await Process.StandardInput.FlushAsync();
    }
}