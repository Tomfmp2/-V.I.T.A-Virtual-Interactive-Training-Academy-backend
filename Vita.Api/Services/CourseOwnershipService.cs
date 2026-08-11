using Vita.Api.Repositories;

namespace Vita.Api.Services;

public class CourseOwnershipService : ICourseOwnershipService
{
    private readonly ICourseOwnershipRepository _courseOwnershipRepository;

    public CourseOwnershipService(ICourseOwnershipRepository courseOwnershipRepository)
    {
        _courseOwnershipRepository = courseOwnershipRepository;
    }

    public Task<bool> IsCourseOwnerAsync(string userId, int courseId)
    {
        if (string.IsNullOrEmpty(userId))
            return Task.FromResult(false);

        return _courseOwnershipRepository.ExistsCourseOwnedByAsync(userId, courseId);
    }
}
