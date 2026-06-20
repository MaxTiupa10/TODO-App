using TODO_App.Services.DTOs;

namespace TODO_App.Services.Interfaces;

public interface ITaskService
{
    Task<PagedResult<TaskDto>> GetTasksAsync(
        int userId,
        int pageNumber,
        int pageSize,
        int? categoryId,
        string? searchQuery,
        string? listType,
        DateTime? deadlineFromUtc,
        DateTime? deadlineToUtc);
    Task<TaskDto?> GetTaskByIdAsync(int userId, int taskId);
    Task<TaskDto> CreateTaskAsync(int userId, CreateTaskDto dto);
    Task<bool> UpdateTaskAsync(int userId, int taskId, UpdateTaskDto dto);
    Task<bool> DeleteTaskAsync(int userId, int taskId);
}
