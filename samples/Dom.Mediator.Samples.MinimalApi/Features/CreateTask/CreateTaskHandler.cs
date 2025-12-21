using Dom.Mediator;
using Dom.Mediator.Abstractions;
using Dom.Mediator.Samples.MinimalApi.Features;

public class CreateTaskHandler : ICommandHandler<CreateTaskCommand, string>
{
    private readonly TaskRepository _taskRepostiroy;

    public CreateTaskHandler(TaskRepository taskRepository) => _taskRepostiroy = taskRepository;

    public async Task<Result<string>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var validation = Validate(request);

        if (validation.Count > 0)
        {
            Error error = new Error(TaskItem.ErrorCodes.CREATE_TASK, "Invalid fields upon creation", TaskItem.ErrorTypes.VALIDATION);
            error.AddDetails(validation);

            return Result<string>.Failure(error);
        }

        var createTask = TaskItem.Create(request.Title, request.Description, request.DueDate);

        if(createTask.IsFailure)
        {
            return Result<string>.Failure(createTask.Error!);
        }

        var task = createTask.Value!;

        _taskRepostiroy.Tasks.Add(task);

        return Result<string>.Success(task.Id);
    }

    public List<ErrorDetail> Validate(CreateTaskCommand request)
    {
        List<ErrorDetail> errors = new();

        if (string.IsNullOrWhiteSpace(request.Title))
            errors.Add(new ErrorDetail("title", "Title is required."));

        if (string.IsNullOrWhiteSpace(request.Description))
            errors.Add(new ErrorDetail("description", "Description is required."));

        return errors;
    }
}