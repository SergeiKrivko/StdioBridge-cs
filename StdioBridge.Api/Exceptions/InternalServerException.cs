namespace StdioBridge.Api.Exceptions;

public class InternalServerException : BridgeException
{
    public override int Code => 500;
    
    public InternalServerException()
    {
    }

    public InternalServerException(string message) : base(message)
    {
    }
}