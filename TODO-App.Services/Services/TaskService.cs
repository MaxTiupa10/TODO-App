using TODO_App.DataAccess.Interfaces;
using TODO_App.Domain.Entities;
using TODO_App.Services.DTOs;


namespace TODO_App.Services.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;

    public TaskService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<IEnumerable<TaskDto>> GetTasksAsync(int userId, int pageNumber, int pageSize, int? categoryId, string? searchQuery)
    {
        var tasks = await _taskRepository.GetTaskAsync(userId, pageNumber, pageSize, categoryId, searchQuery);
        
        // Мапимо сутності БД на DTO
        return tasks.Select(t => new TaskDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            IsCompleted = t.IsCompleted,
            CreatedAt = t.CreatedAt,
            CategoryId = t.CategoryId,
            CategoryName = t.Category?.Name
        });
    }

    public async Task<TaskDto?> GetTaskByIdAsync(int userId, int taskId)
    {
        var task = await _taskRepository.GetTaskByIdAsync(userId, taskId);
        if (task == null) return null;

        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            IsCompleted = task.IsCompleted,
            CreatedAt = task.CreatedAt,
            CategoryId = task.CategoryId,
            CategoryName = task.Category?.Name
        };
    }

    public async Task<TaskDto> CreateTaskAsync(int userId, CreateTaskDto dto)
    {
        var task = new ToDoTask
        {
            UserId = userId,
            Title = dto.Title,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        await _taskRepository.AddTaskAsync(task);
        await _taskRepository.SaveChangesAsync();

        // Повертаємо створену таску у вигляді DTO (можна було б підтягнути категорію, але для простоти просто мапимо)
        return new TaskDto 
        { 
            Id = task.Id, Title = task.Title, Description = task.Description, 
            IsCompleted = task.IsCompleted, CreatedAt = task.CreatedAt, CategoryId = task.CategoryId 
        };
    }

    public async Task<bool> UpdateTaskAsync(int userId, int taskId, UpdateTaskDto dto)
    {
        var task = await _taskRepository.GetTaskByIdAsync(userId, taskId);
        if (task == null) return false; // Таски немає, або вона належить іншому юзеру

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.IsCompleted = dto.IsCompleted;
        task.CategoryId = dto.CategoryId;

        _taskRepository.UpdateTaskAsync(task);
        await _taskRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTaskAsync(int userId, int taskId)
    {
        var task = await _taskRepository.GetTaskByIdAsync(userId, taskId);
        if (task == null) return false;

        _taskRepository.DeleteTaskAsync(task);
        await _taskRepository.SaveChangesAsync();
        return true;
    }
}