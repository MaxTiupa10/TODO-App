using TODO_App.Domain.Entities;

namespace TODO_App.DataAccess.Interfaces;

public interface ITaskRepository
{
    Task<IEnumerable<ToDoTask>> GetTaskAsync(int userId,int pageNumber, int pageSize, int? categoryId, string? searchQuery);
    Task<ToDoTask?> GetTaskByIdAsync(int userId,int taskId);
    Task AddTaskAsync(ToDoTask task);
    void UpdateTaskAsync(ToDoTask task);
    void DeleteTaskAsync(ToDoTask task);
    Task SaveChangesAsync();
}