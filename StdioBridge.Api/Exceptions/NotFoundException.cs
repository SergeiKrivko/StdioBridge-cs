namespace StdioBridge.Api.Exceptions;

public class NotFoundException : BridgeException
{
    public override int Code => 404;
}