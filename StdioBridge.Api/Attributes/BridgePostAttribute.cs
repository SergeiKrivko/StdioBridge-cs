namespace StdioBridge.Api.Attributes;

public class BridgePostAttribute : BridgeMethodAttribute
{
    public override string Method => "post";

    public BridgePostAttribute(string url) : base(url)
    {
        
    }
    
    public BridgePostAttribute() : base()
    {
        
    }
}