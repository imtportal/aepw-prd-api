using AEPW.Application.DTOs.Authorization;

namespace AEPW.Application.Interfaces.Repositories.Authorization
{
    public interface IAuthorizationUserRepository
    {
        Task<LoginContextDto?> GetLoginContextAsync(
            Guid azureAdObjectId,
            CancellationToken cancellationToken = default);

        Task<PageAccessDto> GetAppUserPageAccessAsync(
            string pageKey,
            long userId,
            CancellationToken cancellationToken = default);
        Task<List<ActionAccessDto>> GetAppUserActionAccessAsync(
    string pageKey,
    long userId,
    CancellationToken cancellationToken = default);
    }
}
