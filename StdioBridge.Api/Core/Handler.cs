using System.Reflection;
using System.Text.Json;
using StdioBridge.Api.Attributes;
using StdioBridge.Api.Exceptions;

namespace StdioBridge.Api.Core;

internal class Handler
{
    private MethodInfo _method;
    private object _controller;
    private List<Parameter> _parameters = [];

    public Handler(object controller, MethodInfo method)
    {
        _controller = controller;
        _method = method;

        foreach (var param in _method.GetParameters())
        {
            _parameters.Add(new Parameter(param));
        }
    }

    public Task<object?> Call(string data, Dictionary<string, string> pathParams, Dictionary<string, string> queryParams)
    {
        var parameters = new List<object>();
        foreach (var parameter in _parameters)
        {
            switch (parameter.Type)
            {
                case Parameter.ParameterType.Body:
                    var body = JsonSerializer.Deserialize(data,
                        typeof(RequestBodyModel<>).MakeGenericType(parameter.DataType));
                    if (body == null)
                        throw new UnprocessableEntityException($"Request body can not be deserialized as '{parameter.DataType}'");
                    parameters.Add(ExtractData(body));
                    break;
                case Parameter.ParameterType.Path:
                    parameters.Add(parameter.FromString(pathParams.GetValueOrDefault(parameter.Name)));
                    break;
                case Parameter.ParameterType.Query:
                    parameters.Add(parameter.FromString(queryParams.GetValueOrDefault(parameter.Name)));
                    break;
            }
        }

        var task = _method.Invoke(_controller, parameters.ToArray()) as Task ?? throw new Exception("Invalid return type");
        return ExecuteTask(task);
    }

    private async Task<object?> ExecuteTask(dynamic task)
    {
        return await task;
    }

    private object ExtractData(dynamic request)
    {
        return request.Data;
    }
}