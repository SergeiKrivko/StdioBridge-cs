namespace StdioBridge.Api.Exceptions;

public abstract class BridgeException : Exception
{
    public abstract int Code { get; }
    
    protected BridgeException()
    {
    }

    protected BridgeException(string message) : base(message)
    {
    }
}