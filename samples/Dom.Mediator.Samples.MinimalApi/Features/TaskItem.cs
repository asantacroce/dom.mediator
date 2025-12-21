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
    // Centralized error-code and type constants (UPPERCASE_SEPARATED_BY_UNDERSCORE)
    public static class ErrorCodes
    {
        public const string CREATE_001 = "CREATE_001";
        public const string CREATE_TASK = "CREATE_TASK";

        public const string UPDATE_001 = "UPDATE_001";
        public const string UPDATE_002 = "UPDATE_002";
        public const string UPDATE_003 = "UPDATE_003";
        public const string UPDATE_004 = "UPDATE_004";

        // Additional codes used by handlers
        public const string UPDATE_TASK_NOT_FOUND = "UPDATE_TASK_NOT_FOUND";
        public const string UPDATE_COMMENT_REQUIRED = "UPDATE_COMMENT_REQUIRED";
    }

    public static class ErrorTypes
    {
        public const string VALIDATION = "validation";
        public const string INVALID_OPERATION = "invalid_operation";
        public const string NOT_FOUND = "not_found";
    }

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
                return Result<TaskItem>.Failure(ErrorCodes.CREATE_001, "Due date must be in the future.", ErrorTypes.INVALID_OPERATION);
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
                ErrorCodes.UPDATE_001,
                "A comment is required when updating the task status",
                ErrorTypes.VALIDATION));
        }

        if (this.Status == Status.Created)
        {
            return Result.Failure(new Error(
                ErrorCodes.UPDATE_002,
                "Cannot set status to Created",
                ErrorTypes.INVALID_OPERATION));
        }

        if (this.Status == Status.Completed)
        {
            return Result.Failure(new Error(
                ErrorCodes.UPDATE_003,
                "Completed tasks cannot be updated",
                ErrorTypes.INVALID_OPERATION));
        }

        if (this.Status == Status.InProgress && newStatus == Status.Completed)
        {
            return Result.Failure(new Error(
                ErrorCodes.UPDATE_004,
                "Task status cannot be moved directly from InProgress to Completed without being first Tested",
                ErrorTypes.INVALID_OPERATION));
        }

        Status = newStatus;

        return Result.Success();
    }
}