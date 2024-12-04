namespace StdioBridge.Api.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class BridgeControllerAttribute : Attribute
{
    public string Url { get; }

    public BridgeControllerAttribute(string url = "")
    {
        Url = url.Trim('/');
    }

    // public BridgeControllerAttribute()
    // {
    // }
}