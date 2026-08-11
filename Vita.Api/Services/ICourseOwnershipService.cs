namespace Vita.Api.Services;

public interface ICourseOwnershipService
{
    Task<bool> IsCourseOwnerAsync(string userId, int courseId);
}
