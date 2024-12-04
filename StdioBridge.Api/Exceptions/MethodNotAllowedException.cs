namespace StdioBridge.Api.Exceptions;

public class MethodNotAllowedException : BridgeException
{
    public override int Code => 405;
}