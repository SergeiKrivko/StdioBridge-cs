namespace StdioBridge.Api.Attributes;

public class BridgeGetAttribute : BridgeMethodAttribute
{
    public override string Method => "get";

    public BridgeGetAttribute(string url) : base(url)
    {
        
    }
    
    public BridgeGetAttribute() : base()
    {
        
    }
}