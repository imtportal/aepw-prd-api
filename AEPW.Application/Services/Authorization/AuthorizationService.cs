using AEPW.Application.DTOs.Authorization;
using AEPW.Application.Interfaces.Repositories.Authorization;
using AEPW.Application.Interfaces.Services.Authorization;

namespace AEPW.Application.Services.Authorization
{
    public class AuthorizationService : IAuthorizationUserService
    {
        private readonly IAuthorizationUserRepository _authorizationRepository;

        public AuthorizationService(IAuthorizationUserRepository authorizationRepository)
        {
            _authorizationRepository = authorizationRepository;
        }

        public Task<LoginContextDto?> GetLoginContextAsync(
            Guid azureAdObjectId,
            CancellationToken cancellationToken = default)
        {
            return _authorizationRepository.GetLoginContextAsync(azureAdObjectId, cancellationToken);
        }

        public Task<PageAccessDto> GetAppUserPageAccessAsync(
            string pageKey,
            long userId,
            CancellationToken cancellationToken = default)
        {
            return _authorizationRepository.GetAppUserPageAccessAsync(
                pageKey,
                userId,
                cancellationToken);
        }
        public Task<List<ActionAccessDto>> GetAppUserActionAccessAsync(
    string pageKey,
    long userId,
    CancellationToken cancellationToken = default)
        {
            return _authorizationRepository.GetAppUserActionAccessAsync(
                pageKey,
                userId,
                cancellationToken);
        }
    }
}
