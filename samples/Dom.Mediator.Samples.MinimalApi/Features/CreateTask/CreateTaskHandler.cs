using Dom.Mediator;
using Dom.Mediator.Abstractions;

public class CreateTaskHandler : ICommandHandler<CreateTaskCommand>
{
    private readonly TaskStore _store;

    public CreateTaskHandler(TaskStore store) => _store = store;

    public Task<Result> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        if(Validate(request) is Error error)
        {
            return Task.FromResult(Result.Failure(error));
        }

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate,
            CreatedAt = DateTime.UtcNow,
            IsCompleted = false
        };

        _store.Tasks.Add(task);
        return Task.FromResult(Result.Success());
    }

    public Error Validate(CreateTaskCommand request)
    {
        Error error = new Error("CREATE_001", "Invalid fields upon creation", "validation");

        if (string.IsNullOrWhiteSpace(request.Title))
            error.AddDetail("title", "Title is required.");

        if (request.DueDate.HasValue && request.DueDate.Value < DateTime.UtcNow)
            error.AddDetail("dueDate", "Due date cannot be in the past.");

        if (error.Details.Count > 0)
            {
            return error;
        }
        return null;
    }
}