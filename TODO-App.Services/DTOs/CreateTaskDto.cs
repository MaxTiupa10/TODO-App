namespace TODO_App.Services.DTOs;

public class CreateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? CategoryId { get; set; }
    public bool IsImportant { get; set; }
    public DateTime? Deadline { get; set; }
}