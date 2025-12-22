using Dom.Mediator.Abstractions;

public record GetAllTasksQuery() : IQuery<List<TaskItem>>;