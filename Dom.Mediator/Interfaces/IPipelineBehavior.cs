using Dom.Mediator.Models;
using Dom.Mediator.ResultPattern;

namespace Dom.Mediator.Interfaces;

/// <summary>
/// Defines a behavior in the processing pipeline for handling requests and responses.
/// </summary>
/// <remarks>Implementations of this interface can be used to add cross-cutting concerns, such as logging,
/// validation, or exception handling,  to the request processing pipeline. The behavior is invoked before and/or after
/// the request handler executes.</remarks>
/// <typeparam name="TRequest">The type of the request being processed. Must implement <see cref="IRequest{TResponse}"/>.</typeparam>
/// <typeparam name="TResponse">The type of the response returned by the request handler.</typeparam>
public interface IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next);
}

public interface IPipelineBehavior<TRequest>
    where TRequest : ICommand
{
    Task<Result> Handle(TRequest request, CancellationToken cancellationToken, CommandHandlerDelegate next);
}