using Dom.Mediator.Interfaces;
using Dom.Mediator.Interfaces.Handlers;
using Dom.Mediator.Models;
using Dom.Mediator.ResultPattern;
using System.Reflection;

namespace Dom.Mediator;

public class Mediator : IMediator
{
    private readonly Dictionary<Type, object> _queryHandlers = new();
    private readonly Dictionary<Type, object> _commandHandlers = new();

    private readonly Dictionary<Type, List<object>> _behaviours = new();

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

    public void AddRequestResponseBehaviour<TRequest, TResponse>(IPipelineBehavior<TRequest, TResponse> behaviour)
        where TRequest : IRequest<TResponse>
    {
        AddBehaviour<TRequest>(behaviour);
    }

    /// <summary>
    /// Adds a behavior for commands without a response
    /// </summary>
    public void AddRequestBehaviour<TCommand>(IPipelineBehavior<TCommand> behaviour)
        where TCommand : ICommand
    {
        AddBehaviour<TCommand>(behaviour);
    }
    #endregion

    #region MEDIATOR METHODS
    public async Task<Result<TResponse>> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var handlerRes = GetHandlerType(request);

        RequestHandlerDelegate<TResponse> next = () => handlerRes.Item2.Handle((dynamic)request, cancellationToken);

        // Apply behaviors using the generic method
        next = ApplyBehaviours(handlerRes.Item1, request, cancellationToken, next);
        
        return await next();
    }

    /// <summary>
    /// Sends a command that doesn't return a value
    /// </summary>
    public async Task<Result> Send(ICommand command, CancellationToken cancellationToken = default)
    {
        var handlerRes = GetHandlerType(command);

        CommandHandlerDelegate next = () => handlerRes.Item2.Handle((dynamic)command, cancellationToken);

        // Apply behaviors using the generic method
        next = ApplyBehaviours(handlerRes.Item1, command, cancellationToken, next);
        
        return await next();
    }
    #endregion

    #region PRIVATE
    private RequestHandlerDelegate<TResponse> ApplyBehaviours<TRequest, TResponse>(
        Type requestType, 
        TRequest request, 
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
        where TRequest : IRequest<TResponse>
    {
        // Apply behaviors in reverse order (last registered = first executed)
        if (_behaviours.TryGetValue(requestType, out var behaviours))
        {
            foreach (var behaviour in behaviours.Reverse<object>())
            {
                var currentNext = next;
                var typedBehaviour = (dynamic)behaviour;
                next = () => typedBehaviour.Handle((dynamic)request, cancellationToken, currentNext);
            }
        }

        return next;
    }

    private CommandHandlerDelegate ApplyBehaviours<TCommand>(
        Type requestType, 
        TCommand command, 
        CancellationToken cancellationToken,
        CommandHandlerDelegate next)
        where TCommand : ICommand
    {
        // Apply behaviors in reverse order (last registered = first executed)
        if (_behaviours.TryGetValue(requestType, out var behaviours))
        {
            foreach (var behaviour in behaviours.Reverse<object>())
            {
                var currentNext = next;
                var typedBehaviour = (dynamic)behaviour;
                next = () => typedBehaviour.Handle((dynamic)command, cancellationToken, currentNext);
            }
        }

        return next;
    }
    
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
    
    private void AddBehaviour<T>(object behaviour)
    {
        var commandType = typeof(T);

        if (!_behaviours.TryGetValue(commandType, out var behaviours))
        {
            behaviours = new List<object>();
            _behaviours[commandType] = behaviours;
        }

        behaviours.Add(behaviour);
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
