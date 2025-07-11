using Dom.Mediator.Interfaces;
using Dom.Mediator.Interfaces.Handlers;
using Dom.Mediator.Models;
using Dom.Mediator.ResultPattern;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Dom.Mediator;

internal class Mediator : IMediator
{
    private readonly Dictionary<Type, object> _queryHandlers = new();
    private readonly Dictionary<Type, object> _commandHandlers = new();
    private readonly List<Type> _requestResponseBehaviours = new();
    private readonly List<Type> _commandBehaviours = new();
    private readonly IServiceProvider? _serviceProvider;

    public Mediator(IServiceProvider? serviceProvider = null)
    {
        _serviceProvider = serviceProvider;
    }

    public IEnumerable<Type> HandledRequestTypes { get { return _queryHandlers.Keys.Union(_commandHandlers.Keys); } }

    #region INIT
    public void ScanHandlers(params Assembly[] assemblies)
    {
        var types = assemblies.SelectMany(a => a.GetTypes())
                              .Where(t => !t.IsAbstract && !t.IsInterface)
                              .ToList();

        foreach (var type in types)
        {
            foreach (var iface in type.GetInterfaces())
            {
                if (!iface.IsGenericType) continue;

                var def = iface.GetGenericTypeDefinition();

                if (
                    def == typeof(IQueryHandler<,>)
                    )
                {
                    var requestType = iface.GetGenericArguments()[0];
                    _queryHandlers[requestType] = Activator.CreateInstance(type)!;
                }
                else if (
                    def == typeof(ICommandHandler<>) ||
                    def == typeof(ICommandHandler<,>)
                    )
                {
                    var commandType = iface.GetGenericArguments()[0];
                    _commandHandlers[commandType] = Activator.CreateInstance(type)!;
                }
            }
        }
    }

    public void AddRequestResponseBehaviour(Type behaviourType)
    {
        if (!behaviourType.IsGenericType)
            throw new ArgumentException("Behaviour type must be generic", nameof(behaviourType));

        var genericTypeDefinition = behaviourType.GetGenericTypeDefinition();
        _requestResponseBehaviours.Add(genericTypeDefinition);
    }

    public void AddCommandBehaviour(Type behaviourType)
    {
        if (!behaviourType.IsGenericType)
            throw new ArgumentException("Behaviour type must be generic", nameof(behaviourType));

        var genericTypeDefinition = behaviourType.GetGenericTypeDefinition();
        _commandBehaviours.Add(genericTypeDefinition);
    }
    #endregion

    #region MEDIATOR METHODS
    public async Task<Result<TResponse>> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var handlerRes = GetHandlerType(request);
        var requestType = handlerRes.Item1;

        RequestHandlerDelegate<TResponse> next = () => handlerRes.Item2.Handle((dynamic)request, cancellationToken);

        // Apply behaviors in reverse order (so the first registered behavior is the outermost)
        foreach (var behaviourType in _requestResponseBehaviours.AsEnumerable().Reverse())
        {
            var concreteBehaviourType = behaviourType.MakeGenericType(requestType, typeof(TResponse));
            
            // Create behavior instance using service provider if available
            object? behaviour;
            if (_serviceProvider != null)
            {
                behaviour = ActivatorUtilities.CreateInstance(_serviceProvider, concreteBehaviourType);
            }
            else
            {
                behaviour = Activator.CreateInstance(concreteBehaviourType);
            }

            var currentNext = next;
            next = () => ((dynamic)behaviour).Handle((dynamic)request, cancellationToken, currentNext);
        }
        
        return await next();
    }

    /// <summary>
    /// Sends a command that doesn't return a value
    /// </summary>
    public async Task<Result> Send(ICommand command, CancellationToken cancellationToken = default)
    {
        var handlerRes = GetHandlerType(command);
        var commandType = handlerRes.Item1;

        CommandHandlerDelegate next = () => handlerRes.Item2.Handle((dynamic)command, cancellationToken);

        // Apply behaviors in reverse order (so the first registered behavior is the outermost)
        foreach (var behaviourType in _commandBehaviours.AsEnumerable().Reverse())
        {
            var concreteBehaviourType = behaviourType.MakeGenericType(commandType);
            
            // Create behavior instance using service provider if available
            object? behaviour;
            if (_serviceProvider != null)
            {
                behaviour = ActivatorUtilities.CreateInstance(_serviceProvider, concreteBehaviourType);
            }
            else
            {
                behaviour = Activator.CreateInstance(concreteBehaviourType);
            }

            var currentNext = next;
            next = () => ((dynamic)behaviour).Handle((dynamic)command, cancellationToken, currentNext);
        }
        
        return await next();
    }
    #endregion

    #region PRIVATE
    
    
    private dynamic GetHandler(Type requestType)
    {
        if (_queryHandlers.TryGetValue(requestType, out var queryObj))
        {
            return (dynamic)queryObj;
        }
        else
        {
            if (_commandHandlers.TryGetValue(requestType, out var handlerObj))
            {
                return (dynamic)handlerObj;
            }
        }

        return null;
    }
    
    private (Type, dynamic) GetHandlerType(object request)
    {
        var requestType = request.GetType();
        dynamic handler = GetHandler(requestType);

        if (handler is null)
            throw new ApplicationException($"Handler not registered for type: {requestType.Name}");

        return (requestType, handler);
    }

    
    #endregion
}
