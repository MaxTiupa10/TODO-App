using TODO_App.Services.DTOs;

namespace TODO_App.Services.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllAsync(int userId);
    Task<CategoryDto?> GetByIdAsync(int userId, int categoryId);
    Task<CategoryDto> CreateAsync(int userId, CreateCategoryDto dto);
    Task<bool> UpdateAsync(int userId, int categoryId, UpdateCategoryDto dto);
    Task<bool> DeleteAsync(int userId, int categoryId);
}
