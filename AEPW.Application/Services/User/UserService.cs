using AEPW.Application.DTOs.Common;
using AEPW.Application.Exceptions;
using AEPW.Application.Interfaces.Repositories.Review;
using AEPW.Application.Interfaces.Repositories.User;
using AEPW.Application.Interfaces.Services.AzureAd;
using AEPW.Application.Interfaces.Services.User;
using AEPW.Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AEPW.Application.Services.User
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAzureAdService _azureAdService;

        public UserService(IUserRepository userRepository, IAzureAdService azureAdService)
        {
            _userRepository = userRepository;
            _azureAdService = azureAdService;
        }

        public Task<UserDetailDto?> GetUserByIdAsync(
            long userId,
            CancellationToken cancellationToken = default)
        {
            return _userRepository.GetUserByIdAsync(userId, cancellationToken);
        }

        public Task<IReadOnlyList<UserDto>> GetUserByEmail(string email)
        {
            return _userRepository.GetUserByEmail(email);
        }

        public Task<PaginatedResult<UserListDto>> GetAllUsersAsync(
            string? searchText = null,
            long? systemRoleId = null,
            bool? isActive = null,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            return _userRepository.GetAllUsersAsync(
                searchText,
                systemRoleId,
                isActive,
                pageNumber,
                pageSize,
                cancellationToken);
        }

        public Task<CreateUserResponseDto> CreateUserAsync(
            CreateUserRequestDto request,
            CancellationToken cancellationToken = default)
        {
            return _userRepository.CreateUserAsync(request, cancellationToken);
        }

        public Task<UpdateUserResponseDto> UpdateUserAsync(
            UpdateUserRequestDto request,
            CancellationToken cancellationToken = default)
        {
            return _userRepository.UpdateUserAsync(request, cancellationToken);
        }

        public Task<DeleteUserResponseDto> DeleteUserAsync(
            DeleteUserRequestDto request,
            CancellationToken cancellationToken = default)
        {
            return _userRepository.DeleteUserAsync(request, cancellationToken);
        }

        public Task<IReadOnlyList<UserRoleSummaryDto>> GetRoleSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            return _userRepository.GetRoleSummaryAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<BulkImportUserResultDto>> BulkImportUsersAsync(
            BulkImportUserRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (request?.Users == null || request.Users.Count == 0)
            {
                throw new ValidationException("At least one user is required.");
            }

            var results = new List<BulkImportUserResultDto>();

            var emails = request.Users
                .Select(u => u.Email)
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var azureAdResponse = await _azureAdService.GetUsersByEmailsAsync(emails);
            var azureAdUsers = azureAdResponse.Data ?? new List<AEPW.Application.DTOs.AzureAdUser.AzureAdUserLookupDto>();

            var recordsToImport = new List<AppUserBulkImportRecordDto>();

            foreach (var item in request.Users)
            {
                var email = item.Email?.Trim();

                if (string.IsNullOrWhiteSpace(email))
                {
                    results.Add(new BulkImportUserResultDto
                    {
                        Email = email ?? string.Empty,
                        Status = "Skipped",
                        Reason = "Email is required."
                    });
                    continue;
                }

                var azureAdUser = azureAdUsers.FirstOrDefault(u =>
                    string.Equals(u.Mail, email, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(u.UserPrincipalName, email, StringComparison.OrdinalIgnoreCase));

                if (azureAdUser == null || !Guid.TryParse(azureAdUser.Id, out var azureAdObjectId))
                {
                    results.Add(new BulkImportUserResultDto
                    {
                        Email = email,
                        Status = "Skipped",
                        Reason = "Azure AD user not found."
                    });
                    continue;
                }

                recordsToImport.Add(new AppUserBulkImportRecordDto
                {
                    AzureAdObjectId = azureAdObjectId,
                    Upn = azureAdUser.UserPrincipalName ?? email,
                    Email = azureAdUser.Mail ?? email,
                    DisplayName = azureAdUser.DisplayName ?? email,
                    FirstName = azureAdUser.GivenName,
                    LastName = azureAdUser.Surname,
                    Phone = azureAdUser.MobilePhone,
                    JobTitle = azureAdUser.JobTitle,
                    SystemRoleCode = item.SystemRoleCode
                });
            }

            if (recordsToImport.Count > 0)
            {
                var importResults = await _userRepository.BulkImportUsersAsync(
                    recordsToImport,
                    request.CreatedBy,
                    cancellationToken);

                results.AddRange(importResults);
            }

            return results;
        }
    }
}
