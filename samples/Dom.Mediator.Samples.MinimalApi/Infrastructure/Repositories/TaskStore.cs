public enum Status
{ 
    Created,
    InProgress,
    ReadyToTest,
    Completed
}

public class TaskItem
{
    public required string Id { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public Status Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<Comment> Comments { get; } = new();

    public void AddComment(string text)
    {
        Comments.Add(new Comment(text, DateTime.UtcNow));
    }
}

public class TaskStore
{
    public List<TaskItem> Tasks { get; } = new();
}

public record Comment(string Text, DateTime CreatedAt);