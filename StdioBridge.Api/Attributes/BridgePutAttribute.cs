namespace StdioBridge.Api.Attributes;

public class BridgePutAttribute : BridgeMethodAttribute
{
    public override string Method => "put";

    public BridgePutAttribute(string url) : base(url)
    {
        
    }
    
    public BridgePutAttribute() : base()
    {
        
    }
}