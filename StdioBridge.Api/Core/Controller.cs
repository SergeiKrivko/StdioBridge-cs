using StdioBridge.Api.Attributes;

namespace StdioBridge.Api.Core;

internal class Controller
{
    public string? Url { get; }
    public Router Router { get; }
    private Type _type;
    private object _controller;
    
    public Controller(object controller)
    {
        _type = controller.GetType();
        _controller = controller;
        var attr = _type.GetCustomAttributes(typeof(BridgeControllerAttribute), true).First();
        if (attr is not BridgeControllerAttribute attribute)
            throw new Exception();
        Url = attribute.Url;
        Router = new Router(Url?.Split('/').Length > 0 ? Url.Split('/')[^1] : "");
        AddHandlers();
    }

    private void AddHandlers()
    {
        foreach (var method in _type.GetMethods()
                     .Where(t => t.GetCustomAttributes(typeof(BridgeMethodAttribute), true).Length > 0))
        {
            if (method.GetCustomAttributes(typeof(BridgeMethodAttribute), true).First() is BridgeMethodAttribute attribute)
            {
                var handler = new Handler(_controller, method);
                Router.Add(attribute.Url?.Split('/').ToList() ?? [], attribute.Method, handler);
            }
        }
    }
    
    
}