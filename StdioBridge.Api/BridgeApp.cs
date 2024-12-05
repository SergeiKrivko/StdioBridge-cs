using System.Reflection;
using System.Text.Json;
using System.Web;
using StdioBridge.Api.Attributes;
using StdioBridge.Api.Core;
using StdioBridge.Api.Exceptions;

namespace StdioBridge.Api;

public class BridgeApp
{
    private readonly Router _rootRouter = new Router("");

    public async Task RunAsync()
    {
        while (true)
        {
            var line = await Task.Run(Console.ReadLine);
            if (!string.IsNullOrEmpty(line))
            {
                if (line.StartsWith("!exit!"))
                    break;
                ProcessRequest(line);
            }
        }
    }

    private RequestModel GetRequest(string line)
    {
        RequestModel? request;
        try
        {
            request = JsonSerializer.Deserialize<RequestModel>(line);
        }
        catch (JsonException e)
        {
            throw new BadRequestException(e.Message);
        }

        if (request == null)
            throw new BadRequestException();
        return request;
    }

    private (string, Dictionary<string, string>) ParseQuery(string url)
    {
        var prefix = "https://example.com/";
        var uri = new Uri(prefix + url);
        var query = HttpUtility.ParseQueryString(uri.Query);
        var res = new Dictionary<string, string>();
        foreach (var key in query.AllKeys)
        {
            var value = query[key];
            if (key != null && value != null)
                res.Add(key, value);
        }

        return (uri.AbsolutePath, res);
    }

    private const string ResponsePrefix = "!response!";
    private const string StreamStartPrefix = "!stream_start!";
    private const string StreamChunkPrefix = "!stream_chunk!";
    private const string StreamEndPrefix = "!stream_end!";

    private async void ProcessRequest(string line)
    {
        try
        {
            var request = GetRequest(line);
            if (request.Stream)
            {
                await ProcessStreamRequest(line, request);
            }
            else
            {
                await ProcessSimpleRequest(line, request);
            }
        }
        catch (BadRequestException)
        {
        }
    }

    private async Task ProcessSimpleRequest(string line, RequestModel request)
    {
        try
        {
            var (url, queryParams) = ParseQuery(request.Url);
            var (handler, pathParams) = _rootRouter.Found(url, request.Method);
            try
            {
                var res = await handler.Call(line, pathParams, queryParams);
                Console.WriteLine(ResponsePrefix + JsonSerializer.Serialize(
                    new ResponseModel { Id = request.Id, Code = 200, Data = res }));
            }
            catch (BridgeException)
            {
            }
            catch (Exception e)
            {
                throw new InternalServerException(e.Message);
            }
        }
        catch (BridgeException e)
        {
            Console.WriteLine(ResponsePrefix + JsonSerializer.Serialize(new ResponseModel
                { Id = request.Id, Code = e.Code, Data = new { message = e.Message } }));
        }
    }

    private async Task ProcessStreamRequest(string line, RequestModel request)
    {
        var started = false;
        try
        {
            var (url, queryParams) = ParseQuery(request.Url);
            var (handler, pathParams) = _rootRouter.Found(url, request.Method);
            try
            {
                await foreach (var el in handler.CallStream(line, pathParams, queryParams))
                {
                    if (!started)
                    {
                        Console.WriteLine(StreamStartPrefix + JsonSerializer.Serialize(
                            new StreamResponseModel { Id = request.Id, Code = 200 }));
                        started = true;
                    }

                    Console.WriteLine(StreamChunkPrefix + JsonSerializer.Serialize(
                        new StreamChunkModel { Id = request.Id, Data = el }));
                }

                Console.WriteLine(StreamEndPrefix + JsonSerializer.Serialize(
                    new StreamResponseModel { Id = request.Id, Code = 200 }));
            }
            catch (BridgeException)
            {
            }
            catch (Exception e)
            {
                throw new InternalServerException(e.Message);
            }
        }
        catch (BridgeException e)
        {
            if (!started)
                Console.WriteLine(StreamStartPrefix + JsonSerializer.Serialize(new StreamResponseModel
                    { Id = request.Id, Code = e.Code }));
            Console.WriteLine(StreamEndPrefix + JsonSerializer.Serialize(new StreamResponseModel
                { Id = request.Id, Code = e.Code }));
        }
    }

    public BridgeApp AddControllers()
    {
        return AddControllers(Assembly.GetCallingAssembly());
    }

    public BridgeApp AddControllers(Assembly assembly)
    {
        foreach (var controller in GetControllers(assembly).Select(t => new Controller(t)))
        {
            _rootRouter.Add(controller.Url?.Split('/').ToList() ?? [], controller.Router);
        }

        return this;
    }

    private static List<Type> GetControllers(Assembly assembly)
    {
        return GetClassesWithAttribute<BridgeControllerAttribute>(assembly);
    }

    private static List<Type> GetClassesWithAttribute<TAttribute>(Assembly assembly) where TAttribute : Attribute
    {
        var typesWithAttribute = assembly.GetTypes()
            .Where(t => t.GetCustomAttributes(typeof(TAttribute), true).Length > 0).ToList();
        return typesWithAttribute ?? [];
    }
}