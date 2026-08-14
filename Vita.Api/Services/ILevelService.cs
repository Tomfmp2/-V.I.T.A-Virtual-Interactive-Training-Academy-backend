using Vita.Api.Dtos.Levels;

namespace Vita.Api.Services;

public enum LevelOutcome
{
    Success,
    NotFound,
    NombreExists,
    InUse
}

public class LevelResult
{
    public LevelOutcome Outcome { get; set; }
    public LevelResponse? Level { get; set; }
}

public interface ILevelService
{
    Task<List<LevelResponse>> GetAllAsync();
    Task<LevelResponse?> GetByIdAsync(int id);
    Task<LevelResult> CreateAsync(LevelRequest request);
    Task<LevelResult> UpdateAsync(int id, LevelRequest request);
    Task<LevelResult> DeleteAsync(int id);
}
