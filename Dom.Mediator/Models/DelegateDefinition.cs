using Dom.Mediator.ResultPattern;

namespace Dom.Mediator.Models;

public delegate Task<Result<TResponse>> RequestHandlerDelegate<TResponse>();
public delegate Task<Result> CommandHandlerDelegate();