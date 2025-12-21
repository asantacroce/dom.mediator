using Dom.Mediator;
using Dom.Mediator.Abstractions;

public class UpdateTaskHandler : ICommandHandler<UpdateTaskCommand>
{
    private readonly TaskRepository _store;

    public UpdateTaskHandler(TaskRepository store) => _store = store;

    public async Task<Result> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = _store.Tasks.SingleOrDefault(x => x.Id == request.Id);

        if (task is null)
        {
            Error error = new Error("UPDATE_TASK_", "Task not found", "not_found");
            return Result.Failure(error);
        }

        if (request.Status.HasValue)
        {
            task.ChangeStatus(request.Status.Value, request.Comment);
        }
        else
        {
            if (string.IsNullOrEmpty(request.Comment))
            {
                Error error = new Error("UPDATE_002", "Comment not provided", "invalid_operation");
                return Result.Failure(error); 
            }

            task.AddComment(request.Comment);
        }

        return Result.Success();
    }
}