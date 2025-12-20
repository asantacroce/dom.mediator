using Dom.Mediator;
using Dom.Mediator.Abstractions;
using Dom.Mediator.Samples.MinimalApi.Features;

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

        var task = validationResult.Value !;

        if (request.Status.HasValue)
        { 
            task.Status = request.Status.Value;
        }

        task.AddComment(request.Comment);

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

        if (request.Status.HasValue)
        {
            if (request.Status == Status.Created)
            { 
                return Result<TaskItem>.Failure(new Error(
                    "UPDATE_002",
                    "Cannot set status to Created",
                    "invalid_operation"));
            }

            if (task.Status == Status.Completed)
            {
                return Result<TaskItem>.Failure(new Error(
                    "UPDATE_003",
                    "Completed tasks cannot be updated",
                    "invalid_operation"));
            }

            if (task.Status == Status.InProgress && request.Status == Status.Completed)
            {
                return Result<TaskItem>.Failure(new Error(
                    "UPDATE_004",
                    "Task status cannot be moved directly from InProgress to Completed without being first Tested",
                    "invalid_operation"));
            }
        }


        if (string.IsNullOrEmpty(request.Comment))
        {
            return Result<TaskItem>.Failure(new Error(
                "UPDATE_005",
                "At least a comment is required when updating a task",
                "validation"));
        }

        // Validation passed
        return Result<TaskItem>.Success(task);
    }
}