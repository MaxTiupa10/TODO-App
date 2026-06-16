using Microsoft.EntityFrameworkCore;
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
        int userId, int pageNumber, int pageSize, int? categoryId, string? searchQuery)
    {
        var query = _dbContext.Tasks
            .Include(t => t.Category)
            .Where(t => t.UserId == userId)
            .AsQueryable();

        if (categoryId.HasValue)
            query = query.Where(t => t.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            query = query.Where(t => t.Title.Contains(searchQuery) ||
                                     (t.Description != null && t.Description.Contains(searchQuery)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .AsNoTracking()
            .OrderByDescending(t => t.CreatedAt)
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
