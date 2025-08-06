namespace Dom.Mediator;

public delegate Task<Result<TResponse>> RequestHandlerDelegate<TResponse>();
public delegate Task<Result> CommandHandlerDelegate();