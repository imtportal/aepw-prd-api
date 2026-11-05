using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.Common;
using AEPW.Application.DTOs.User;
using AEPW.Application.Interfaces.Services.User;
using AEPW.Application.Interfaces.Services.AzureAd;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace AEPW.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {

        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        private readonly IAzureAdService _azureAdService;
        public UserController(IUserService userService, ILogger<UserController> logger,
                IAzureAdService azureAdService)
        {
            _userService = userService;
            _azureAdService = azureAdService;
            _logger = logger;
        }
        // Gets Azure AD users by Object Ids.

        [HttpPost("azure-ad/users-by-email")]
        public async Task<IActionResult> GetAzureAdUsersByEmail(
            [FromBody] IReadOnlyList<string> emails)
        {
            if (emails == null || emails.Count == 0)
            {
                return BadRequest(ApiResponse<object>.Fail("At least one email is required."));
            }

            var response = await _azureAdService.GetUsersByEmailsAsync(emails);

            return Ok(response);
        }

        // 🔹 Search Azure AD Users
        [HttpGet]
        public async Task<IActionResult> SearchUsers(
         [FromQuery] string term)
        {
            var response =
                await _azureAdService.SearchUsersAsync(term);

            return Ok(response);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetUserById(
            long id,
            CancellationToken cancellationToken = default)
        {
            var result = await _userService.GetUserByIdAsync(id, cancellationToken);

            if (result is null)
                return NotFound(ApiResponse<object>.Fail("User not found."));

            return Ok(ApiResponse<UserDetailDto>.Ok(result));
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] string? searchText,
            [FromQuery] long? systemRoleId,
            [FromQuery] bool? isActive,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var response = await _userService.GetAllUsersAsync(
                searchText,
                systemRoleId,
                isActive,
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(ApiResponse<PaginatedResult<UserListDto>>.Ok(response, "Success"));
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(
            [FromBody] CreateUserRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var response = await _userService.CreateUserAsync(
                request,
                cancellationToken);

            return Ok(ApiResponse<CreateUserResponseDto>.Ok(response, "User created successfully"));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUser(
            [FromBody] UpdateUserRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var response = await _userService.UpdateUserAsync(
                request,
                cancellationToken);

            return Ok(ApiResponse<UpdateUserResponseDto>.Ok(response, "User updated successfully"));
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser(
            [FromBody] DeleteUserRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var response = await _userService.DeleteUserAsync(
                request,
                cancellationToken);

            if (response.HasPendingReviews)
            {
                return BadRequest(ApiResponse<DeleteUserResponseDto>.Fail(
                    $"User cannot be deleted. The user has pending review approvals. Review IDs: {response.PendingReviewIds}",
                    "USER_HAS_PENDING_REVIEWS"
                ));
            }

            return Ok(ApiResponse<DeleteUserResponseDto>.Ok(response, "User deleted successfully"));
        }

        [HttpGet("role-summary")]
        public async Task<IActionResult> GetRoleSummary(
            CancellationToken cancellationToken = default)
        {
            var response = await _userService.GetRoleSummaryAsync(cancellationToken);

            return Ok(ApiResponse<IReadOnlyList<UserRoleSummaryDto>>.Ok(response, "Success"));
        }

        [HttpGet("{emailId}")]
        public async Task<IActionResult> GetUserByEmail(string emailId)
        {
            var response =
                await _userService.GetUserByEmail(emailId);

            return Ok(ApiResponse<IReadOnlyList<UserDto>>.Ok(response, "Success"));
        }

        [HttpPost("import")]
        public async Task<IActionResult> BulkImportUsers(
            [FromBody] BulkImportUserRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var response = await _userService.BulkImportUsersAsync(
                request,
                cancellationToken);

            return Ok(ApiResponse<IReadOnlyList<BulkImportUserResultDto>>.Ok(response, "Bulk import completed."));
        }

    }
}
