namespace StdioBridge.Api.Attributes;

public class BridgePatchAttribute : BridgeMethodAttribute
{
    public override string Method => "patch";

    public BridgePatchAttribute(string url) : base(url)
    {
        
    }
    
    public BridgePatchAttribute() : base()
    {
        
    }
}