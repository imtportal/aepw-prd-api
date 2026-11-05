using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.ReviewTask;
using AEPW.Application.Interfaces.Services.ReviewTask;
using Microsoft.AspNetCore.Mvc;

namespace AEPW.API.Controllers;

[ApiController]
[Route("api/task")]
public class ReviewTaskController : ControllerBase
{
    private readonly IReviewTaskService _reviewTaskService;

    public ReviewTaskController(IReviewTaskService reviewTaskService)
    {
        _reviewTaskService = reviewTaskService;
    }

    [HttpGet]
    public async Task<IActionResult> GetReviewApprovalTasks(
        [FromQuery] GetReviewTasksRequestDto request,
        CancellationToken cancellationToken)
    { 

        if (request.PeriodCode?.ToLower() == "all")
        {
            request.PeriodCode = String.Empty;
        }

        ApiResponse<PagedResponse<ReviewTaskDto>> result = await _reviewTaskService.GetReviewApprovalTasksAsync(
            request,
            cancellationToken);

        return Ok(result);
    }
}
