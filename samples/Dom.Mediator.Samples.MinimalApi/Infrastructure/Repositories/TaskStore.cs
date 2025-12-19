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
}

public class TaskStore
{
    public List<TaskItem> Tasks { get; } = new();
}