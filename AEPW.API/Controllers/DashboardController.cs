using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.Dashboard;
using AEPW.Application.Interfaces.Services.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AEPW.API.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public sealed class DashboardController : ControllerBase
  {
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
      _dashboardService = dashboardService;
    }

    [Authorize]
    [HttpGet("portfolio")]
    public async Task<IActionResult> GetDashboardPortfolio(
        [FromQuery] DashboardPortfolioListRequestDto request,
        CancellationToken cancellationToken)
    {
      var result =
          await _dashboardService.GetDashboardPortfolioListAsync(
              request,
              cancellationToken);

      return Ok(ApiResponse<object>.Ok(result));
    }


    [Authorize]
    [HttpGet("portfolio/adjustment-by-categories")]
    public async Task<IActionResult> GetDashboardPortfolioAdjustmentByCategories(
        [FromQuery] DashboardPortfolioByCategoriesRequestDto request,
        CancellationToken cancellationToken)
    {
      var result =
          await _dashboardService
              .GetDashboardPortfolioAdjustmentByCategoriesAsync(
                  request,
                  cancellationToken);

      return Ok(ApiResponse<object>.Ok(result));
    }
  }
}
