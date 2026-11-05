using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.ReviewTask;

namespace AEPW.Application.Interfaces.Services.ReviewTask;

public interface IReviewTaskService
{
    Task<ApiResponse<PagedResponse<ReviewTaskDto>>> GetReviewApprovalTasksAsync(
        GetReviewTasksRequestDto request,
        CancellationToken cancellationToken = default);
}
