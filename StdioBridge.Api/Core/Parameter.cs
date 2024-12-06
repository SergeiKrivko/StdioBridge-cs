using System.Reflection;
using StdioBridge.Api.Attributes;
using StdioBridge.Api.Exceptions;

namespace StdioBridge.Api.Core;

internal class Parameter
{
    public enum ParameterType
    {
        Body,
        Path,
        Query
    }

    public ParameterType Type { get; }

    public Type DataType { get; }

    public string Name { get; }
    
    public object? Default { get; }

    public Parameter(ParameterInfo info)
    {
        DataType = info.ParameterType;
        Default = info.DefaultValue;
        if (info.GetCustomAttributes(typeof(FromBodyAttribute), true).FirstOrDefault() != null)
        {
            Type = ParameterType.Body;
            Name = "";
        }
        else if (info.GetCustomAttributes(typeof(FromPathAttribute), true).FirstOrDefault() is FromPathAttribute
                 pathAttr)
        {
            Type = ParameterType.Path;
            Name = pathAttr.Name ?? info.Name ?? "";
        }
        else if (info.GetCustomAttributes(typeof(FromQueryAttribute), true).FirstOrDefault() is FromQueryAttribute
                 queryAttr)
        {
            Type = ParameterType.Query;
            Name = queryAttr.Name ?? info.Name ?? "";
        }
        else
        {
            throw new Exception($"Unknown parameter '{info.Name}'");
        }
    }

    public object FromString(string? data)
    {
        if (data == null)
            return Default ?? throw new UnprocessableEntityException($"Param '{Name}' missed");
        return Convert.ChangeType(data, DataType);
    }
}