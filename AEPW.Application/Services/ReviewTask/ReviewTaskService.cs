using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.ReviewTask;
using AEPW.Application.Interfaces.Repositories.ReviewTask;
using AEPW.Application.Interfaces.Services.ReviewTask;

namespace AEPW.Application.Services.ReviewTask;

public class ReviewTaskService : IReviewTaskService
{
    private readonly IReviewTaskRepository _reviewTaskRepository;

    public ReviewTaskService(IReviewTaskRepository reviewTaskRepository)
    {
        _reviewTaskRepository = reviewTaskRepository;
    }

    public async Task<ApiResponse<PagedResponse<ReviewTaskDto>>> GetReviewApprovalTasksAsync(
        GetReviewTasksRequestDto request,
        CancellationToken cancellationToken = default)
    {
        return await _reviewTaskRepository.GetReviewApprovalTasksAsync(
            request,
            cancellationToken);
    }
}
