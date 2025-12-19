using Dom.Mediator.Abstractions;
using Dom.Mediator.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Reflection;

namespace Dom.Mediator.Implementation;

public class Mediator : IMediator
{
    private const int BEHAVIOUR_COMMAND_ARGS = 1;
    private const int BEHAVIOUR_QUERY_ARGS = 2;

    private readonly Dictionary<Type, Type> _queryHandler = new();
    private readonly Dictionary<Type, Type> _commandHandler = new();
    private readonly IServiceProvider _serviceProvider;

    private readonly List<Type> _behaviours = new();

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    #region INIT
    public void RegisterHandlers(params Assembly[] assemblies)
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

                if (def == typeof(IQueryHandler<,>))
                {
                    var requestType = iface.GetGenericArguments()[0];

                    if (requestType.IsNotPublic)
                        throw new MediatorException($"The type '{type}' which defines the 'Query' must be declared as public.");

                    if (type.IsNotPublic)
                        throw new MediatorException($"The type '{type}' which implements a 'Query Handler' must be declared as public.");

                    _queryHandler[requestType] = type;
                }
                else if (
                    def == typeof(ICommandHandler<>) ||
                    def == typeof(ICommandHandler<,>))
                {
                    var commandType = iface.GetGenericArguments()[0];

                    if (commandType.IsNotPublic)
                        throw new MediatorException($"The type '{commandType}' which defines the 'Command' must be declared as public.");

                    if (type.IsNotPublic)
                        throw new MediatorException($"The type '{type}' which implements a 'Command Handler' must be declared as public.");

                    _commandHandler[commandType] = type;
                }
            }
        }
    }

    public void AddBehaviour(Type behaviourType)
    {
        if (!behaviourType.IsGenericType)
            throw new ArgumentException("Behaviour type must be generic", nameof(behaviourType));

        var genericTypeDefinition = behaviourType.GetGenericTypeDefinition();
        _behaviours.Add(genericTypeDefinition);
    }

    #endregion

    #region MEDIATOR METHODS
    public async Task<Result<TResponse>> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var handlerRes = GetHandlerType(request);
        var requestType = handlerRes.Item1;

        RequestHandlerDelegate<TResponse> next = () => handlerRes.Item2.Handle((dynamic)request, cancellationToken);

        // Apply behaviors in reverse order (so the first registered behavior is the outermost)
        foreach (var behaviourType in _behaviours.AsEnumerable().Reverse())
        {
            // Check if this behavior has the right arity (2 type parameters)
            if (behaviourType.GetGenericArguments().Length == BEHAVIOUR_QUERY_ARGS)
            {
                var concreteBehaviourType = behaviourType.MakeGenericType(requestType, typeof(TResponse));

                object? behaviour = ActivatorUtilities.CreateInstance(_serviceProvider, concreteBehaviourType);

                var currentNext = next;
                next = () => ((dynamic)behaviour).Handle((dynamic)request, cancellationToken, currentNext);
            }
            // Skip behaviors with different arity
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
        foreach (var behaviourType in _behaviours.AsEnumerable().Reverse())
        {
            // Check if this behavior has the right arity (1 type parameter)
            if (behaviourType.GetGenericArguments().Length == BEHAVIOUR_COMMAND_ARGS)
            {
                var concreteBehaviourType = behaviourType.MakeGenericType(commandType);

                object? behaviour = ActivatorUtilities.CreateInstance(_serviceProvider, concreteBehaviourType);

                var currentNext = next;
                next = () => ((dynamic)behaviour).Handle((dynamic)command, cancellationToken, currentNext);
            }
            // Skip behaviors with different arity
        }

        return await next();
    }
    #endregion

    #region PRIVATE

    private dynamic GetHandler(Type requestType)
    {
        if (_queryHandler.TryGetValue(requestType, out var handlerType))
        {
            return ActivatorUtilities.CreateInstance(_serviceProvider, handlerType);
        }
        else if (_commandHandler.TryGetValue(requestType, out var commandHandlerType))
        {
            return ActivatorUtilities.CreateInstance(_serviceProvider, commandHandlerType);
        }

        throw new MediatorException($"Handler not registered for type: {requestType.Name}");
    }

    private (Type, dynamic) GetHandlerType(object request)
    {
        var requestType = request.GetType();
        dynamic handler = GetHandler(requestType);

        if (handler is null)
            throw new MediatorException($"Handler not registered for type: {requestType.Name}");

        return (requestType, handler);
    }


    #endregion
}