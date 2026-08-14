namespace Vita.Api.Repositories;

public interface ICourseOwnershipRepository
{
    Task<bool> ExistsCourseOwnedByAsync(string userId, int courseId);
}
