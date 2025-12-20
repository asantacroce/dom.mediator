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
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public Status Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<Comment> Comments { get; } = new();

    public static Result<TaskItem> Create(string title, string description, DateTime dueDate)
    {
        if (DateTime.UtcNow.Subtract(dueDate).TotalHours < 0)
        {
            return Result<TaskItem>.Failure("TASK_001", "Due date must be in the future.", "invalid_operation");
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
}
