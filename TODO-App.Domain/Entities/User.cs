namespace TODO_App.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty; 
    public ICollection<ToDoTask> Tasks { get; set; } = new List<ToDoTask>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
}