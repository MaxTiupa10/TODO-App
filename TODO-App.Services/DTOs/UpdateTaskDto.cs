namespace TODO_App.Services.DTOs;

public class UpdateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsImportant { get; set; }
    public DateTime? Deadline { get; set; }
    public int? CategoryId { get; set; }
}