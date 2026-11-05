using AEPW.Application.DTOs.Common;
using AEPW.Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.Interfaces.Repositories.User
{
    public interface IUserRepository
    {
        Task<UserDetailDto?> GetUserByIdAsync(
            long userId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<UserDto>> GetUserByEmail(string email, CancellationToken cancellationToken = default);
        Task<PaginatedResult<UserListDto>> GetAllUsersAsync(
            string? searchText = null,
            long? systemRoleId = null,
            bool? isActive = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        Task<CreateUserResponseDto> CreateUserAsync(
            CreateUserRequestDto request,
            CancellationToken cancellationToken = default);

        Task<UpdateUserResponseDto> UpdateUserAsync(
            UpdateUserRequestDto request,
            CancellationToken cancellationToken = default);

        Task<DeleteUserResponseDto> DeleteUserAsync(
            DeleteUserRequestDto request,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<UserRoleSummaryDto>> GetRoleSummaryAsync(
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<BulkImportUserResultDto>> BulkImportUsersAsync(
            IReadOnlyList<AppUserBulkImportRecordDto> users,
            string createdBy,
            CancellationToken cancellationToken = default);
    }
}
