using Dom.Mediator;
using Dom.Mediator.Abstractions;

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

        var task = new TaskItem
        {
            Id = Guid.NewGuid().ToString(),
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate,
            CreatedAt = DateTime.UtcNow,
            Status = Status.Created
        };

        _store.Tasks.Add(task);
        return Task.FromResult(Result<string>.Success(task.Id));
    }

    public List<ErrorDetail> Validate(CreateTaskCommand request)
    {
        List<ErrorDetail> errors = [];

        if (string.IsNullOrWhiteSpace(request.Title))
            errors.Add(new ("title", "Title is required."));

        if (request.DueDate.HasValue && request.DueDate.Value < DateTime.UtcNow)
            errors.Add(new ("dueDate", "Due date cannot be in the past."));

        return errors;
    }
}