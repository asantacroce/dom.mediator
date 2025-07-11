using Dom.Mediator.Interfaces;
using Dom.Mediator.ResultPattern;

namespace Dom.Mediator;

/// <summary>
/// Defines the mediator interface for sending requests/commands to their handlers
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Sends a request to a single handler
    /// </summary>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <param name="request">Request object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>Result containing the response or error details</returns>
    Task<Result<TResponse>> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sends a command that doesn't return a value
    /// </summary>
    /// <param name="command">Command object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result> Send(ICommand command, CancellationToken cancellationToken = default);
}