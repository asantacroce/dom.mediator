using Dom.Mediator.Abstractions;

public record UpdateTaskCommand(
    string Id,
    Status Status) : ICommand;