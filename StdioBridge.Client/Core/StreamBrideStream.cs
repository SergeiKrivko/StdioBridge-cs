using System.Text;

namespace StdioBridge.Client.Core;

internal class StreamBrideStream : IBrideClientStream
{
    private readonly Stream _stream;

    public event Action<string>? OnNewLine;
    public async Task WriteLineAsync(string line)
    {
        await _stream.WriteAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(line)));
        await _stream.FlushAsync();
    }

    public StreamBrideStream(Stream stream)
    {
        _stream = stream;
    }
    
    private async void ReadOutputLoop()
    {
        while (_stream.CanRead)
        {
            var memory = new Memory<byte>();
            var count = await _stream.ReadAsync(memory);
            if (count > 0)
            {
                var line = Encoding.UTF8.GetString(memory.ToArray());
                OnNewLine?.Invoke(line);
            }
        }
    }

}