namespace StdioBridge.Api.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public abstract class BridgeMethodAttribute : Attribute
{
    public string? Url { get; }
    public abstract string Method { get; }
    
    protected BridgeMethodAttribute(string url)
    {
        Url = url.Trim('/');
    }
    
    protected BridgeMethodAttribute()
    {
    }
}