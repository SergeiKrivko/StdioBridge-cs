namespace StdioBridge.Api.Core;

public class BridgeAppServices
{
    private BridgeApp _app;
    private readonly List<object> _services = [];

    internal BridgeAppServices(BridgeApp app)
    {
        _app = app;
    }

    public BridgeApp Add(Type serviceType)
    {
        _services.Add(CreateServiceInstance(serviceType));
        return _app;
    }

    public BridgeApp Add<T>()
    {
        _services.Add(CreateServiceInstance(typeof(T)));
        return _app;
    }

    public object Get(Type serviceType)
    {
        foreach (var service in _services)
        {
            if (serviceType.IsInstanceOfType(service))
                return service;
        }

        throw new Exception($"Service of type '{serviceType}' not found");
    }

    public T Get<T>()
    {
        if (Get(typeof(T)) is T service)
            return service;
        throw new Exception($"Service of type '{typeof(T)}' not found");
    }

    internal object CreateServiceInstance(Type serviceType)
    {
        var constructor = serviceType.GetConstructors().First(c => c.IsPublic);
        var parameters = new List<object?>();
        foreach (var parameter in constructor.GetParameters())
        {
            try
            {
                parameters.Add(Get(parameter.ParameterType));
            }
            catch (Exception)
            {
                if (parameter.HasDefaultValue)
                    parameters.Add(parameter.DefaultValue);
                else
                    throw;
            }
        }

        return Activator.CreateInstance(serviceType, parameters.ToArray()) ??
               throw new Exception($"Can not create instance of '{serviceType}'");
    }
}