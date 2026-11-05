using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.ReviewTask;

namespace AEPW.Application.Interfaces.Repositories.ReviewTask;

public interface IReviewTaskRepository
{
    Task<ApiResponse<PagedResponse<ReviewTaskDto>>> GetReviewApprovalTasksAsync(
        GetReviewTasksRequestDto request,
        CancellationToken cancellationToken = default);
}
