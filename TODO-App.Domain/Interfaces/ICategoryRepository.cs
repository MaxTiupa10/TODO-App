using TODO_App.Domain.Entities;

namespace TODO_App.Domain.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllAsync(int userId);
    Task<Category?> GetByIdAsync(int userId, int categoryId);
    Task AddAsync(Category category);
    void Update(Category category);
    void Delete(Category category);
    Task SaveChangesAsync();
}
