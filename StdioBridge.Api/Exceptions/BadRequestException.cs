namespace StdioBridge.Api.Exceptions;

public class BadRequestException : BridgeException
{
    public override int Code => 400;

    public BadRequestException()
    {
    }

    public BadRequestException(string message) : base(message)
    {
    }
}