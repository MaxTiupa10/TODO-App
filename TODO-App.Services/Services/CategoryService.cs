using TODO_App.Domain.Entities;
using TODO_App.Domain.Interfaces;
using TODO_App.Services.DTOs;
using TODO_App.Services.Interfaces;

namespace TODO_App.Services.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync(int userId)
    {
        var categories = await _categoryRepository.GetAllAsync(userId);
        return categories.Select(c => new CategoryDto { Id = c.Id, Name = c.Name });
    }

    public async Task<CategoryDto?> GetByIdAsync(int userId, int categoryId)
    {
        var category = await _categoryRepository.GetByIdAsync(userId, categoryId);
        return category == null ? null : new CategoryDto { Id = category.Id, Name = category.Name };
    }

    public async Task<CategoryDto> CreateAsync(int userId, CreateCategoryDto dto)
    {
        var category = new Category
        {
            UserId = userId,
            Name = dto.Name
        };

        await _categoryRepository.AddAsync(category);
        await _categoryRepository.SaveChangesAsync();

        return new CategoryDto { Id = category.Id, Name = category.Name };
    }

    public async Task<bool> UpdateAsync(int userId, int categoryId, UpdateCategoryDto dto)
    {
        var category = await _categoryRepository.GetByIdAsync(userId, categoryId);
        if (category == null) return false;

        category.Name = dto.Name;
        _categoryRepository.Update(category);
        await _categoryRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int userId, int categoryId)
    {
        var category = await _categoryRepository.GetByIdAsync(userId, categoryId);
        if (category == null) return false;

        _categoryRepository.Delete(category);
        await _categoryRepository.SaveChangesAsync();
        return true;
    }
}
