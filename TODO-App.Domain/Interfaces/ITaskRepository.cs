using TODO_App.Domain.Entities;

namespace TODO_App.Domain.Interfaces;

public interface ITaskRepository
{
    Task<(IEnumerable<ToDoTask> Items, int TotalCount)> GetTasksAsync(
        int userId, int pageNumber, int pageSize, int? categoryId, string? searchQuery);
    Task<ToDoTask?> GetTaskByIdAsync(int userId, int taskId);
    Task AddTaskAsync(ToDoTask task);
    void UpdateTask(ToDoTask task);
    void DeleteTask(ToDoTask task);
    Task SaveChangesAsync();
}
