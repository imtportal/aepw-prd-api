using AEPW.Application.DTOs.Report;
using AEPW.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using AEPW.Application.Interfaces.Services.Report;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
namespace AEPW.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
  private readonly IReportService _reportService;

  public ReportController(
      IReportService reportService)
  {
    _reportService = reportService;
  }

  [Authorize]
  [HttpPost("generate")]

  public async Task<IActionResult> GenerateReport(
    [FromBody] ReportRequestDto request,
    [FromHeader(Name = "UserId")] string userId,
    CancellationToken cancellationToken)
  {
    request.UserId = userId;

    var result = await _reportService.GenerateReportAsync(
        request,
        cancellationToken
    );

    return File(
        result.Excel,
        result.ContentType,
        result.FileName
    );
  }

  [Authorize]
  [HttpGet("metric-filter-options")]
  public async Task<IActionResult> ReportMetricFilterOptions(
    [FromQuery] MetricFilterRequestDto request,
    CancellationToken cancellationToken)
  {
    var result =
        await _reportService.ReportMetricFilterOptions(
            request,
            cancellationToken);

    return Ok(result);
  }

  [Authorize]
  [HttpGet("report-filter-project-cascading-options")]
  public async Task<IActionResult> ReportFilterProjectCascadingOptions(
    [FromQuery] ReportFilterProjectCascadingRequestDto request,
    CancellationToken cancellationToken)
  {
    var result =
        await _reportService.ReportFilterProjectCascadingOptions(
            request,
            cancellationToken);

    return Ok(result);
  }

  [Authorize]
  [HttpGet("report-independent-filter-options")]
  public async Task<IActionResult> ReportIndependentFilterOptions(
      [FromQuery] List<string>? projectIds,
      CancellationToken cancellationToken)
  {
    var result =
        await _reportService.ReportIndependentFilterOptions(
            projectIds,
            cancellationToken);

    return Ok(result);
  }
}

