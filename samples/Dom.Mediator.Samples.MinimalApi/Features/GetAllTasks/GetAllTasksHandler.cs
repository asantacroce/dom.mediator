using Dom.Mediator;
using Dom.Mediator.Abstractions;

public class GetAllTasksHandler : IQueryHandler<GetAllTasksQuery, List<TaskItem>>
{
    private readonly TaskRepository _store;

    public GetAllTasksHandler(TaskRepository store) => _store = store;

    public Task<Result<List<TaskItem>>> Handle(GetAllTasksQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<List<TaskItem>>.Success(_store.Tasks.ToList()));
    }
}