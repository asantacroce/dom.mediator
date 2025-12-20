using Dom.Mediator.Abstractions;
using Dom.Mediator.Samples.MinimalApi.Features;

public record UpdateTaskCommand(
    string Id,
    string Comment,
    Status? Status
    ) : ICommand;