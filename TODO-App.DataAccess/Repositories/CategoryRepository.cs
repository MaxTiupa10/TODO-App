using Microsoft.EntityFrameworkCore;
using TODO_App.Domain.Entities;
using TODO_App.Domain.Interfaces;

namespace TODO_App.DataAccess.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _dbContext;

    public CategoryRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Category>> GetAllAsync(int userId)
    {
        return await _dbContext.Categories
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int userId, int categoryId)
    {
        return await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.UserId == userId && c.Id == categoryId);
    }

    public async Task AddAsync(Category category) => await _dbContext.Categories.AddAsync(category);

    public void Update(Category category) => _dbContext.Categories.Update(category);

    public void Delete(Category category) => _dbContext.Categories.Remove(category);

    public async Task SaveChangesAsync() => await _dbContext.SaveChangesAsync();
}
