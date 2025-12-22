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
            Error error = new Error(TaskItem.ErrorCodes.UPDATE_TASK_NOT_FOUND, $"Task not found for id: {request.Id}", TaskItem.ErrorTypes.NOT_FOUND);
            return Result.Failure(error);
        }

        if (request.Status.HasValue)
        {
            var statusChange = task.ChangeStatus(request.Status.Value, request.Comment);

            if (statusChange.IsFailure)
            {
                return statusChange;
            }
        }

        if (string.IsNullOrEmpty(request.Comment))
        {
            Error error = new Error(TaskItem.ErrorCodes.UPDATE_COMMENT_REQUIRED, "Comment not provided", TaskItem.ErrorTypes.INVALID_OPERATION);
            return Result.Failure(error);
        }

        task.AddComment(request.Comment);


        return Result.Success();
    }
}