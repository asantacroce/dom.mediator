using Dom.Mediator.Abstractions;
using Dom.Mediator.Samples.MinimalApi.Features;

public record GetAllTasksQuery() : IQuery<List<TaskItem>>;