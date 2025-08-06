using Dom.Mediator.Abstractions;

public record CreateTaskCommand(
    string Title, 
    string? Description, 
    DateTime? DueDate) : ICommand;