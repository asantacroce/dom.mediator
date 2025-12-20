using Dom.Mediator;
using Dom.Mediator.Abstractions;
using Dom.Mediator.Samples.MinimalApi.Features;

public class GetAllTasksHandler : IQueryHandler<GetAllTasksQuery, List<TaskItem>>
{
    private readonly TaskStore _store;

    public GetAllTasksHandler(TaskStore store) => _store = store;

    public Task<Result<List<TaskItem>>> Handle(GetAllTasksQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<List<TaskItem>>.Success(_store.Tasks.ToList()));
    }
}