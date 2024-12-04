namespace StdioBridge.Api.Attributes;

public class BridgeDeleteAttribute : BridgeMethodAttribute
{
    public override string Method => "delete";

    public BridgeDeleteAttribute(string url) : base(url)
    {
        
    }
    
    public BridgeDeleteAttribute() : base()
    {
        
    }
}