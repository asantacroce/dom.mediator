namespace Dom.Mediator.Abstractions;

/// <summary>
/// Represents a query that returns a specific response type
/// </summary>
/// <typeparam name="TResponse">The type of response returned by the query</typeparam>
public interface IQuery<TResponse> : IRequest<TResponse>
{
}