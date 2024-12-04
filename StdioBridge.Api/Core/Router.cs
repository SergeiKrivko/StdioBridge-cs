using StdioBridge.Api.Exceptions;

namespace StdioBridge.Api.Core;

internal class Router
{
    private string Name { get; set; }
    private readonly Dictionary<string, Router> _routes = new Dictionary<string, Router>();
    private Router? _pathParamRoute = null;
    private readonly Dictionary<string, Handler> _handlers = new Dictionary<string, Handler>();

    public Router(string name)
    {
        Name = name;
    }

    public void Add(List<string> path, string method, Handler func)
    {
        if (path.Count == 0)
        {
            if (!_handlers.TryAdd(method, func))
            {
                throw new ArgumentException($"Method '{method}' is already registered.");
            }
        }
        else
        {
            var param = path[0].StartsWith('{') ? path[0].Trim('{', '}') : null;
            if (param == null)
            {
                if (!_routes.TryGetValue(path[0], out var router))
                {
                    _routes[path[0]] = router = new Router(path[0]);
                }

                path.RemoveAt(0);
                router.Add(path, method, func);
            }
            else
            {
                _pathParamRoute ??= new Router(param);
                path.RemoveAt(0);
                _pathParamRoute.Add(path, method, func);
            }
        }
    }

    public void Add(List<string> path, Router router)
    {
        if (path.Count == 1)
        {
            if (!_routes.ContainsKey(path[0]))
            {
                _routes[path[0]] = router;
                router.Name = path[0];
            }
            else
            {
                throw new ArgumentException($"Router '{path[0]}' is already registered.");
            }
        }
        else
        {
            if (!_routes.ContainsKey(path[0]))
            {
                _routes[path[0]] = new Router(path[0]);
            }

            var nextRouter = _routes[path[0]];
            path.RemoveAt(0);
            nextRouter.Add(path, router);
        }
    }

    public (Handler, Dictionary<string, string>) Found(string path, string method)
    {
        return _Found(path.Trim('/').Split('/').ToList(), method, new Dictionary<string, string>());
    }

    private (Handler, Dictionary<string, string>) _Found(List<string> path, string method,
        Dictionary<string, string> pathParams)
    {
        if (path.Count == 0)
        {
            if (!_handlers.TryGetValue(method, out var handler))
            {
                throw new MethodNotAllowedException();
            }

            return (handler, pathParams);
        }

        string name = path[0];
        path.RemoveAt(0);
        if (_routes.TryGetValue(name, out var route))
        {
            try
            {
                return route._Found(path, method, pathParams);
            }
            catch (NotFoundException)
            {
            }
        }

        if (_pathParamRoute != null)
        {
            try
            {
                pathParams[_pathParamRoute.Name] = name;
                return _pathParamRoute._Found(path, method, pathParams);
            }
            catch (NotFoundException)
            {
                pathParams.Remove(_pathParamRoute.Name);
            }
        }
        
        throw new NotFoundException();
    }
}