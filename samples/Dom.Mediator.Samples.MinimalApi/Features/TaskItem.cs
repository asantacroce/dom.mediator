using Dom.Mediator.Abstractions;
using System.Threading.Tasks;

namespace Dom.Mediator.Samples.MinimalApi.Features;

public enum Status
{
    Created,
    InProgress,
    ReadyToTest,
    Completed
}

public record Comment(string Text, DateTime CreatedAt);

public class TaskItem
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public DateTime? DueDate { get; private set; }
    public Status Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public List<Comment> Comments { get; } = new();

    public static Result<TaskItem> Create(string title, string description, DateTime? dueDate)
    {
        if (dueDate.HasValue)
        {
            if (DateTime.UtcNow.Subtract(dueDate.Value).TotalHours < 0)
            {
                return Result<TaskItem>.Failure("CREATE_001", "Due date must be in the future.", "invalid_operation");
            }
        }

        var task = new TaskItem
        {
            Id = Guid.NewGuid().ToString(),
            Title = title,
            Description = description,
            DueDate = dueDate,
            CreatedAt = DateTime.UtcNow,
            Status = Status.Created
        };

        return Result<TaskItem>.Success(task);
    }

    public void AddComment(string text)
    {
        Comments.Add(new Comment(text, DateTime.UtcNow));
    }

    public Result ChangeStatus(Status newStatus, string comment)
    {
        if (string.IsNullOrEmpty(comment))
        {
            return Result.Failure(new Error(
                "UPDATE_001",
                "A comment is required when updating the task status",
                "validation"));
        }

        if (this.Status == Status.Created)
        {
            return Result.Failure(new Error(
                "UPDATE_002",
                "Cannot set status to Created",
                "invalid_operation"));
        }

        if (this.Status == Status.Completed)
        {
            return Result.Failure(new Error(
                "UPDATE_003",
                "Completed tasks cannot be updated",
                "invalid_operation"));
        }

        if (this.Status == Status.InProgress && newStatus == Status.Completed)
        {
            return Result.Failure(new Error(
                "UPDATE_004",
                "Task status cannot be moved directly from InProgress to Completed without being first Tested",
                "invalid_operation"));
        }

        Status = newStatus;

        return Result.Success();
    }
}