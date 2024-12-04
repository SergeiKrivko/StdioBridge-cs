namespace StdioBridge.Api.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class FromPathAttribute : Attribute
{
    public string? Name { get; }

    public FromPathAttribute()
    {
        Name = null;
    }

    public FromPathAttribute(string name)
    {
        Name = name;
    }
}