using Vita.Api.Dtos.Categories;

namespace Vita.Api.Services;

public enum CategoryOutcome
{
    Success,
    NotFound,
    NombreExists,
    InUse
}

public class CategoryResult
{
    public CategoryOutcome Outcome { get; set; }
    public CategoryResponse? Category { get; set; }
}

public interface ICategoryService
{
    Task<List<CategoryResponse>> GetAllAsync();
    Task<CategoryResponse?> GetByIdAsync(int id);
    Task<CategoryResult> CreateAsync(CategoryRequest request);
    Task<CategoryResult> UpdateAsync(int id, CategoryRequest request);
    Task<CategoryResult> DeleteAsync(int id);
}
