using System.Reflection;
using System.Text.Json;
using StdioBridge.Api.Exceptions;

namespace StdioBridge.Api.Core;

internal class Handler
{
    private readonly MethodInfo _method;
    private readonly object _controller;
    private readonly List<Parameter> _parameters = [];

    public Handler(object controller, MethodInfo method)
    {
        _controller = controller;
        _method = method;

        foreach (var param in _method.GetParameters())
        {
            _parameters.Add(new Parameter(param));
        }
    }

    public async Task<object?> Call(string data, Dictionary<string, string> pathParams,
        Dictionary<string, string> queryParams)
    {
        var parameters = GetParameters(data, pathParams, queryParams);
        if (_method.ReturnType.IsAssignableTo(typeof(Task)))
        {
            var task = _method.Invoke(_controller, parameters) ?? throw new Exception("Invalid return type");
            return await ExecuteTask(task);
        }

        if (_method.ReturnType.IsAssignableTo(typeof(IAsyncEnumerable<object?>)))
        {
            var enumerator = _method.Invoke(_controller, parameters) as IAsyncEnumerable<object?> ??
                             throw new Exception("Invalid return type");
            return await JoinEnumerator(enumerator);
        }

        return await Task.Run(() => _method.Invoke(_controller, parameters));
    }

    public async IAsyncEnumerable<object?> CallStream(string data, Dictionary<string, string> pathParams,
        Dictionary<string, string> queryParams)
    {
        var parameters = GetParameters(data, pathParams, queryParams);
        if (_method.ReturnType.IsAssignableTo(typeof(Task)))
        {
            var task = _method.Invoke(_controller, parameters) ?? throw new Exception("Invalid return type");
            yield return await ExecuteTask(task);
            yield break;
        }

        if (_method.ReturnType.IsAssignableTo(typeof(IAsyncEnumerable<object?>)))
        {
            var enumerator = _method.Invoke(_controller, parameters) as IAsyncEnumerable<object?> ??
                             throw new Exception("Invalid return type");
            await foreach (var el in enumerator)
                yield return el;
            yield break;
        }

        yield return await Task.Run(() => _method.Invoke(_controller, parameters));
    }

    private object[] GetParameters(string data, Dictionary<string, string> pathParams,
        Dictionary<string, string> queryParams)
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
                        throw new UnprocessableEntityException(
                            $"Request body can not be deserialized as '{parameter.DataType}'");
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

        return parameters.ToArray();
    }

    private async Task<object?> ExecuteTask(dynamic task)
    {
        return await task;
    }

    private async Task<List<object?>> JoinEnumerator(IAsyncEnumerable<object?> enumerator)
    {
        var res = new List<object?>();
        await foreach(var el in enumerator)
            res.Add(el);

        return res;
    }

    private object ExtractData(dynamic request)
    {
        return request.Data;
    }
}