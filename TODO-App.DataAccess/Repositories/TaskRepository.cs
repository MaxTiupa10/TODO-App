using Microsoft.EntityFrameworkCore;
using TODO_App.Domain;
using TODO_App.Domain.Entities;
using TODO_App.Domain.Interfaces;

namespace TODO_App.DataAccess.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _dbContext;

    public TaskRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(IEnumerable<ToDoTask> Items, int TotalCount)> GetTasksAsync(
        int userId,
        int pageNumber,
        int pageSize,
        int? categoryId,
        string? searchQuery,
        string? listType,
        DateOnly? dateFrom,
        DateOnly? dateTo)
    {
        var query = _dbContext.Tasks
            .Include(t => t.Category)
            .Where(t => t.UserId == userId)
            .AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(t => t.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var term = searchQuery.Trim().ToLower();
            query = query.Where(t => t.Title.ToLower().Contains(term) ||
                                     (t.Description != null && t.Description.ToLower().Contains(term)));
        }

        if (dateFrom.HasValue)
        {
            var fromUtc = dateFrom.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(t => t.Deadline.HasValue && t.Deadline.Value >= fromUtc);
        }

        if (dateTo.HasValue)
        {
            var toExclusiveUtc = dateTo.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            query = query.Where(t => t.Deadline.HasValue && t.Deadline.Value < toExclusiveUtc);
        }

        var todayStart = DateTime.UtcNow.Date;
        var tomorrowStart = todayStart.AddDays(1);

        query = listType?.ToLowerInvariant() switch
        {
            TaskListTypes.MyDay => query.Where(t =>
                !t.IsCompleted &&
                (
                    (t.Deadline.HasValue && t.Deadline.Value < tomorrowStart) ||
                    (!t.Deadline.HasValue && t.CreatedAt >= todayStart && t.CreatedAt < tomorrowStart)
                )),
            TaskListTypes.Important => query.Where(t => t.IsImportant && !t.IsCompleted),
            TaskListTypes.Planned => query.Where(t =>
                t.Deadline.HasValue &&
                !t.IsCompleted &&
                t.Deadline.Value >= todayStart),
            TaskListTypes.Completed => query.Where(t => t.IsCompleted),
            TaskListTypes.AssignedToMe => query.Where(t => !t.IsCompleted),
            TaskListTypes.Tasks => query,
            _ => query.Where(t => !t.IsCompleted)
        };

        var totalCount = await query.CountAsync();

        var orderedQuery = listType?.ToLowerInvariant() switch
        {
            TaskListTypes.Planned => query
                .OrderBy(t => t.Deadline)
                .ThenByDescending(t => t.CreatedAt),
            TaskListTypes.MyDay => query
                .OrderBy(t => t.Deadline.HasValue && t.Deadline.Value < todayStart ? 0 : 1)
                .ThenBy(t => t.Deadline)
                .ThenByDescending(t => t.CreatedAt),
            _ => query
                .OrderBy(t => t.IsCompleted)
                .ThenBy(t => t.Deadline.HasValue ? 0 : 1)
                .ThenBy(t => t.Deadline)
                .ThenByDescending(t => t.CreatedAt)
        };

        var items = await orderedQuery
            .AsNoTracking()
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<ToDoTask?> GetTaskByIdAsync(int userId, int taskId)
    {
        return await _dbContext.Tasks
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.UserId == userId && t.Id == taskId);
    }

    public async Task AddTaskAsync(ToDoTask task) => await _dbContext.Tasks.AddAsync(task);

    public void UpdateTask(ToDoTask task) => _dbContext.Tasks.Update(task);

    public void DeleteTask(ToDoTask task) => _dbContext.Tasks.Remove(task);

    public async Task SaveChangesAsync() => await _dbContext.SaveChangesAsync();
}
