using Dom.Mediator;
using Dom.Mediator.Abstractions;

public class CreateTaskHandler : ICommandHandler<CreateTaskCommand, string>
{
    private readonly TaskRepository _taskRepostiroy;

    public CreateTaskHandler(TaskRepository taskRepository) => _taskRepostiroy = taskRepository;

    public async Task<Result<string>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var createTask = TaskItem.Create(request.Title, request.Description, request.DueDate);

        if(createTask.IsFailure)
        {
            return Result<string>.Failure(createTask.Error!);
        }

        var task = createTask.Value!;

        _taskRepostiroy.Tasks.Add(task);

        return Result<string>.Success(task.Id);
    }
}