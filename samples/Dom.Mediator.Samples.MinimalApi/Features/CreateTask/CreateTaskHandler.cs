using Dom.Mediator;
using Dom.Mediator.Abstractions;
using Dom.Mediator.Samples.MinimalApi.Features;

public class CreateTaskHandler : ICommandHandler<CreateTaskCommand, string>
{
    private readonly TaskStore _store;

    public CreateTaskHandler(TaskStore store) => _store = store;

    public Task<Result<string>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var validation = Validate(request);

        if (validation.Count > 0)
        {
            Error error = new Error("CREATE_001", "Invalid fields upon creation", "validation");
            error.AddDetails(validation);

            return Task.FromResult(Result<string>.Failure(error));
        }

        var createTask = TaskItem.Create(request.Title, request.Description, request.DueDate);

        if(createTask.IsFailure)
        {
            return Task.FromResult(Result<string>.Failure(createTask.Error!));
        }

        var task = createTask.Value!;

        _store.Tasks.Add(task);

        return Task.FromResult(Result<string>.Success(task.Id));
    }

    public List<ErrorDetail> Validate(CreateTaskCommand request)
    {
        List<ErrorDetail> errors = [];

        if (string.IsNullOrWhiteSpace(request.Title))
            errors.Add(new ("title", "Title is required."));

        if (string.IsNullOrWhiteSpace(request.Description))
            errors.Add(new("description", "Description is required."));

        return errors;
    }
}