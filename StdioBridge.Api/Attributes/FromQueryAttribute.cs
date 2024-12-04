namespace StdioBridge.Api.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class FromQueryAttribute : Attribute
{
    public string? Name { get; }

    public FromQueryAttribute()
    {
        Name = null;
    }

    public FromQueryAttribute(string name)
    {
        Name = name;
    }
}