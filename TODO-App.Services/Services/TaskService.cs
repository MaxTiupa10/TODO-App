using TODO_App.Domain.Entities;
using TODO_App.Domain.Interfaces;
using TODO_App.Services.DTOs;
using TODO_App.Services.Interfaces;

namespace TODO_App.Services.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly ICategoryRepository _categoryRepository;

    public TaskService(ITaskRepository taskRepository, ICategoryRepository categoryRepository)
    {
        _taskRepository = taskRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<PagedResult<TaskDto>> GetTasksAsync(
        int userId,
        int pageNumber,
        int pageSize,
        int? categoryId,
        string? searchQuery,
        string? listType,
        DateTime? deadlineFromUtc,
        DateTime? deadlineToUtc)
    {
        var (tasks, totalCount) = await _taskRepository.GetTasksAsync(
            userId,
            pageNumber,
            pageSize,
            categoryId,
            searchQuery,
            listType,
            deadlineFromUtc,
            deadlineToUtc);

        return new PagedResult<TaskDto>
        {
            Items = tasks.Select(MapToDto),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<TaskDto?> GetTaskByIdAsync(int userId, int taskId)
    {
        var task = await _taskRepository.GetTaskByIdAsync(userId, taskId);
        return task == null ? null : MapToDto(task);
    }

    public async Task<TaskDto> CreateTaskAsync(int userId, CreateTaskDto dto)
    {
        if (!await IsCategoryValidForUserAsync(userId, dto.CategoryId))
            throw new ArgumentException("Category not found or does not belong to you.");

        var task = new ToDoTask
        {
            UserId = userId,
            Title = dto.Title,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            Deadline = dto.Deadline,
            IsImportant = dto.IsImportant,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        await _taskRepository.AddTaskAsync(task);
        await _taskRepository.SaveChangesAsync();

        return MapToDto(task);
    }

    public async Task<bool> UpdateTaskAsync(int userId, int taskId, UpdateTaskDto dto)
    {
        if (!await IsCategoryValidForUserAsync(userId, dto.CategoryId))
            throw new ArgumentException("Category not found or does not belong to you.");

        var task = await _taskRepository.GetTaskByIdAsync(userId, taskId);
        if (task == null) return false;

        task.Title = dto.Title;
        task.Description = dto.Description;

        if (dto.IsCompleted && !task.IsCompleted)
            task.CompletedAt = DateTime.UtcNow;
        else if (!dto.IsCompleted && task.IsCompleted)
            task.CompletedAt = null;

        task.IsCompleted = dto.IsCompleted;
        task.IsImportant = dto.IsImportant;
        task.Deadline = dto.Deadline;
        task.CategoryId = dto.CategoryId;

        _taskRepository.UpdateTask(task);
        await _taskRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTaskAsync(int userId, int taskId)
    {
        var task = await _taskRepository.GetTaskByIdAsync(userId, taskId);
        if (task == null) return false;

        _taskRepository.DeleteTask(task);
        await _taskRepository.SaveChangesAsync();
        return true;
    }

    private async Task<bool> IsCategoryValidForUserAsync(int userId, int? categoryId)
    {
        if (!categoryId.HasValue)
            return true;

        var category = await _categoryRepository.GetByIdAsync(userId, categoryId.Value);
        return category != null;
    }

    private static TaskDto MapToDto(ToDoTask task) => new()
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        IsCompleted = task.IsCompleted,
        CompletedAt = task.CompletedAt,
        IsImportant = task.IsImportant,
        CreatedAt = task.CreatedAt,
        Deadline = task.Deadline,
        CategoryId = task.CategoryId,
        CategoryName = task.Category?.Name
    };
}
