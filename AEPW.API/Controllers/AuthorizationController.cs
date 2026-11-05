using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.Authorization;
using AEPW.Application.Interfaces.Services.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace AEPW.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorizationController : ControllerBase
    {
        private readonly IAuthorizationUserService _authorizationService;
        private readonly ILogger<AuthorizationController> _logger;

        public AuthorizationController(
            IAuthorizationUserService authorizationService,
            ILogger<AuthorizationController> logger)
        {
            _authorizationService = authorizationService;
            _logger = logger;
        }

        /// <summary>
        /// Get login context for an authenticated user including user details, page access, and action permissions
        /// </summary>
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetLoginContext(
            CancellationToken cancellationToken = default)
        {
            var oid = User.FindFirstValue(
                "http://schemas.microsoft.com/identity/claims/objectidentifier");

            if (!Guid.TryParse(oid, out var azureAdObjectId))
            {
                return Unauthorized(ApiResponse<object>.Fail("Invalid or missing Azure AD Object ID."));
            }

            var result = await _authorizationService.GetLoginContextAsync(
                azureAdObjectId,
                cancellationToken);

            if (result is null)
            {
                return NotFound(ApiResponse<object>.Fail("User not found or inactive."));
            }

            return Ok(ApiResponse<LoginContextDto>.Ok(result, "Success"));
        }

        /// <summary>
        /// Get page and action access for a specific user and page
        /// </summary>
        [HttpGet("user/{userId:long}/page/{pageKey}/access")]
        public async Task<IActionResult> GetAppUserPageAndActionAccess(
            string pageKey,
            long userId,
            CancellationToken cancellationToken = default)
        {
            var result = await _authorizationService.GetAppUserPageAccessAsync(
                pageKey,
                userId,
                cancellationToken);

            return Ok(ApiResponse<PageAccessDto>.Ok(result, "Success"));
        }


        [HttpGet("user/{userId:long}/page/{pageKey}/actions")]
        public async Task<IActionResult> GetAppUserActionAccess(
    string pageKey,
    long userId,
    CancellationToken cancellationToken = default)
        {
            var result = await _authorizationService.GetAppUserActionAccessAsync(
                pageKey,
                userId,
                cancellationToken);

            return Ok(
                ApiResponse<List<ActionAccessDto>>.Ok(
                    result,
                    "Success"));
        }
    }
}
