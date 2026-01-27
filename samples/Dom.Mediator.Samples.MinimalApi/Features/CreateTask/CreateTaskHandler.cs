using Dom.Mediator;
using Dom.Mediator.Abstractions;

public class CreateTaskHandler : ICommandHandler<CreateTaskCommand, string>
{
    private readonly TaskRepository _taskRepository;

    public CreateTaskHandler(TaskRepository taskRepository) => _taskRepository = taskRepository;

    public async Task<Result<string>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var createTask = TaskItem.Create(request.Title, request.Description, request.DueDate);

        if(createTask.IsFailure)
        {
            return Result<string>.Failure(createTask.Error!);
        }

        var task = createTask.Value!;

        _taskRepository.Tasks.Add(task);

        return Result<string>.Success(task.Id);
    }
}