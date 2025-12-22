using Dom.Mediator;

public enum Status
{
    Created,
    InProgress,
    ReadyToTest,
    Completed
}

public record Comment(string Text, DateTime CreatedAt);

public record StatusUpdate(Status status, DateTime Timestamp);

public class TaskItem
{
    public static class ErrorCodes
    {
        public const string CREATE_001 = "CREATE_001";
        public const string CREATE_TASK = "CREATE_TASK";

        public const string UPDATE_001 = "UPDATE_001";
        public const string UPDATE_002 = "UPDATE_002";
        public const string UPDATE_003 = "UPDATE_003";
        public const string UPDATE_004 = "UPDATE_004";
        public const string UPDATE_005 = "UPDATE_005";
        public const string UPDATE_006 = "UPDATE_006";

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
    public Status CurrentStatus { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public List<Comment> Comments { get; } = new();
    public List<StatusUpdate> StatusHistory { get; } = new();

    public static Result<TaskItem> Create(string title, string description, DateTime? dueDate)
    {
        if (dueDate.HasValue)
        {
            if (DateTime.UtcNow.Subtract(dueDate.Value).TotalHours > 0)
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
            CurrentStatus = Status.Created,
        };


        task.TrackStatus(task.CurrentStatus);

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
            return BuildFailure(
                ErrorCodes.UPDATE_001,
                "A comment is required when updating the task status");
        }

        if (newStatus == Status.Created)
        {
            return BuildFailure(
                ErrorCodes.UPDATE_002,
                "Cannot set status to Created");
        }

        if (newStatus == CurrentStatus)
        {
            return BuildFailure(
                ErrorCodes.UPDATE_003,
                $"Current status is already {CurrentStatus}, remove status from the update if no transition is required"
                );
        }

        if (this.CurrentStatus == Status.Completed)
        {
            return BuildFailure(
                ErrorCodes.UPDATE_004,
                "Completed tasks cannot be updated");
        }

        if (this.CurrentStatus == Status.Created && newStatus == Status.ReadyToTest)
        {
            return BuildFailure(
                ErrorCodes.UPDATE_005,
                $"Task should first transition to {Status.InProgress} before moving to testing stage");
        }

        if (NotTested() && newStatus == Status.Completed)
        {
            return BuildFailure(
                ErrorCodes.UPDATE_006,
                $"Task cannot be completed without being first tested");
        }

        //If all rules are respected with then update the status
        CurrentStatus = newStatus;
        TrackStatus(CurrentStatus);

        return Result.Success();
    }

    private void TrackStatus(Status newStatus)
    {
        this.StatusHistory.Add(new StatusUpdate(newStatus, DateTime.UtcNow));
    }

    private Result BuildFailure(string errorCode, string errorMessage)
    {
        return Result.Failure(new Error(
                errorCode,
                errorMessage,
                ErrorTypes.INVALID_OPERATION));
    }

    private bool NotTested() => (CurrentStatus == Status.Created || CurrentStatus == Status.InProgress);
}