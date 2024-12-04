namespace StdioBridge.Api.Exceptions;

public class UnprocessableEntityException : BridgeException
{
    public override int Code => 422;
    
    public UnprocessableEntityException()
    {
    }

    public UnprocessableEntityException(string message) : base(message)
    {
    }
}