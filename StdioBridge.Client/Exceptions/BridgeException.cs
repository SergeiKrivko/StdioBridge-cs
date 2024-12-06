namespace StdioBridge.Client.Exceptions;

public class BridgeException : Exception
{
    public int Code { get; }
    
    public BridgeException(int code, string? message = null) : base(message)
    {
        Code = code;
    }
}