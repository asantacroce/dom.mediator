using Dom.Mediator;
using Dom.Mediator.Abstractions;

public class UpdateTaskHandler : ICommandHandler<UpdateTaskCommand>
{
    private readonly TaskStore _store;

    public UpdateTaskHandler(TaskStore store) => _store = store;

    public async Task<Result> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        // Validate the status transition
        var validationResult = Validate(request);

        if (validationResult.IsFailure)
        {
            return Result.Failure(validationResult.Error !);
        }

        validationResult.Value!.Status = request.Status;

        return Result.Success();
    }

    /// <summary>
    /// Validates whether the status transition is allowed based on business rules.
    /// </summary>
    private Result<TaskItem> Validate(UpdateTaskCommand request)
    {
        var task = _store.Tasks.SingleOrDefault(x => x.Id == request.Id);

        if (task is null)
        {
            Error error = new Error("UPDATE_001", "Task not found", "not_found");
            return Result<TaskItem>.Failure(error);
        }

        // Rule 1: Completed tasks cannot be updated
        if (task.Status == Status.Completed)
        {
            return Result<TaskItem>.Failure(new Error(
                "UPDATE_002",
                "Completed tasks cannot be updated",
                "invalid_operation"));
        }

        // Rule 2: Cannot move back to Created from InProgress
        if (task.Status == Status.InProgress && request.Status == Status.Created)
        {
            return Result<TaskItem>.Failure(new Error(
                "UPDATE_003",
                "Task status cannot be moved back to Created from InProgress",
                "invalid_operation"));
        }

        // Rule 3: Cannot move directly from InProgress to Completed
        if (task.Status == Status.InProgress && request.Status == Status.Completed)
        {
            return Result<TaskItem>.Failure(new Error(
                "UPDATE_004",
                "Task status cannot be moved directly from InProgress to Completed",
                "invalid_operation"));
        }

        // Validation passed
        return Result<TaskItem>.Success(task);
    }
}