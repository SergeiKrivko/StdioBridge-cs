namespace StdioBridge.Client.Core;

public interface IBrideClientStream
{
    public event Action<string>? OnNewLine; 
    public Task WriteLineAsync(string line);
}