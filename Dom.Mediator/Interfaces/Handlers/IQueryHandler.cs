using Dom.Mediator.ResultPattern;

namespace Dom.Mediator.Interfaces.Handlers;

/// <summary>
/// Defines a handler for a query that returns a response
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public interface IQueryHandler<TRequest, TResponse>
    where TRequest : IQuery<TResponse>
{
    Task<Result<TResponse>> Handle(TRequest request, CancellationToken cancellationToken);
}
