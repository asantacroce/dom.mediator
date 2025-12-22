using Dom.Mediator.Abstractions;

public record UpdateTaskCommand(
    string Id,
    string Comment,
    Status? Status
    ) : ICommand;