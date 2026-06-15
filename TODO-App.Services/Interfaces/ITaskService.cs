using TODO_App.Services.DTOs;

namespace TODO_App.DataAccess.Interfaces;

public interface ITaskService
{
    Task<IEnumerable<TaskDto>> GetTasksAsync(int userId, int pageNumber, int pageSize, int? categoryId, string? searchQuery);
    Task<TaskDto?> GetTaskByIdAsync(int userId, int taskId);
    Task<TaskDto> CreateTaskAsync(int userId, CreateTaskDto dto);
    Task<bool> UpdateTaskAsync(int userId, int taskId, UpdateTaskDto dto);
    Task<bool> DeleteTaskAsync(int userId, int taskId);
}