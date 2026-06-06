using TODO_App.Services.DTOs;

namespace TODO_App.DataAccess.Interfaces;

public interface ITaskService
{
    Task<IEnumerable<TaskDto>> GetUserTasksAsync(int userId, int pageNumber, int pageSize, int? categoryId, string? searchQuery);
}