namespace Dom.Mediator.Abstractions;

/// <summary>
/// Represents a command that performs an action but doesn't return a value
/// </summary>
public interface ICommand { }

/// <summary>
/// Represents a command that returns a specific response type
/// </summary>
/// <typeparam name="TResponse"></typeparam>
public interface ICommand<TResponse> : IRequest<TResponse> { }