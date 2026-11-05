using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.BusinessRoleMapping;
using AEPW.Application.DTOs.Lookup;
using AEPW.Application.DTOs.Review;
using AEPW.Application.Interfaces.Services.Lookup;
using Microsoft.AspNetCore.Mvc;
namespace AEPW.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class LookupController : ControllerBase
{
    private readonly ILookupService _lookupService;

    public LookupController(ILookupService lookupService)
    {
        _lookupService = lookupService;
    }

    [HttpGet("app-users")]
    public async Task<IActionResult> GetAllAppUsers([FromQuery] string? searchText, CancellationToken cancellationToken)
    {
        var response = await _lookupService.GetAllAppUsersAsync(searchText, cancellationToken);
        return Ok(response);
    }

    [HttpGet("cities")]
    public async Task<IActionResult> GetCities()
    {
        var response = await _lookupService.GetAllCitiesAsync();
        return Ok(response);
    }
    [HttpGet("record-types")]
    public async Task<IActionResult> GetRecordTypes()
    {
        var response = await _lookupService.GetAllRecordTypesAsync();
        return Ok(response);
    }
    [HttpGet("programs")]
    public async Task<IActionResult> GetAllPrograms([FromQuery] string? searchTerm)
    {
        var response = await _lookupService.GetAllProgramsAsync(searchTerm);
        return Ok(response);
    }
    [HttpGet("metric-sources")]
    public async Task<IActionResult> GetAllMetricSources()
    {
        var response = await _lookupService.GetAllMetricSourcesAsync();
        return Ok(response);
    }

    [HttpGet("statuses/{module}")]
    public async Task<IActionResult> GetStatusesByModule(
        [FromRoute] string module,
        [FromQuery] long? impactMasterId)
    {
        string pageKey = String.Empty;

        if (!String.IsNullOrEmpty(Request.Headers["pageKey"]))
        {
            pageKey = Request.Headers["pageKey"].ToString();
        }

        var response = await _lookupService.GetStatusesByModuleAsync(
            module,
            pageKey,
            impactMasterId);

        return Ok(response);
    }

    [HttpGet("metric-categories")]
    public async Task<IActionResult> GetAllMetricCategories()
    {
        var response = await _lookupService.GetAllMetricCategoriesAsync();
        return Ok(response);
    }

    [HttpGet("metric-units")]
    public async Task<IActionResult> GetAllMetricUnits()
    {
        var response = await _lookupService.GetAllMetricUnitsAsync();
        return Ok(response);
    }

    [HttpGet("parent-metrics")]
    public async Task<IActionResult> GetAllParentMetrics()
    {
        var response = await _lookupService.GetAllParentMetricsAsync();
        return Ok(response);
    }


    [HttpGet("funding-recipients")]
    public async Task<IActionResult> GetAllFundingRecipients()
    {
        var response = await _lookupService.GetAllFundingRecipientsAsync();
        return Ok(response);
    }
    [HttpGet("system-roles")]
    public async Task<IActionResult> GetAllSystemRoles()
    {
        var response = await _lookupService.GetAllSystemRolesAsync();
        return Ok(response);
    }
    [HttpGet("business-roles")]
    public async Task<IActionResult> GetBusinessRoles()
    {
        var result = await _lookupService.GetAllBusinessRolesAsync();

        return Ok(result);
    }

    [HttpGet("business-role-users")]
    public async Task<IActionResult> GetBusinessRoleUsers(
        [FromQuery] string? roleCode,
        [FromQuery] long? impactMasterId,
        CancellationToken cancellationToken)
    {
        var response = await _lookupService.GetBusinessRoleUsersAsync(roleCode, impactMasterId,cancellationToken);
        return Ok(response);
    }

    [HttpGet("project-managers")]
    public async Task<IActionResult> GetProjectManagers(
        [FromQuery] string? searchTerm)
    {
        var response = await _lookupService.GetProjectManagersAsync(searchTerm);

        return Ok(response);
    }
    [HttpGet("impact-categories")]
    public async Task<IActionResult> GetImpactCategoryLookup()
    {
        var response = await _lookupService.GetImpactCategoryLookupAsync();

        return Ok(response);
    }
    [HttpGet("impact-masters")]
    public async Task<IActionResult> GetImpactMasterLookup(
    [FromQuery] string? searchTerm)
    {
        var response =
            await _lookupService.GetImpactMasterLookupAsync(searchTerm);

        return Ok(response);
    }
    [HttpGet("phases")]
    public async Task<IActionResult> GetPhaseLookup()
    {
        var response = await _lookupService.GetPhaseLookupAsync();

        return Ok(response);
    }
    [HttpGet("GetReviewStages")]
    public async Task<IActionResult> GetReviewStages()
    {
        IReadOnlyList<ReviewStageLookupDto> response =
            await _lookupService.GetReviewStages();

        return Ok(ApiResponse<IReadOnlyList<ReviewStageLookupDto>>.Ok(response, "Success"));
    }
    [HttpGet("timezones")]
    public async Task<ActionResult<ApiResponse<List<TimeZoneLookupDto>>>> GetTimeZones([FromQuery] string? searchTerm)
    {
        var response = await _lookupService.GetAllTimeZonesAsync(searchTerm);
        return Ok(response);
    }

    [HttpGet("GetReviewStagesByUserId/{userId}")]
    public async Task<IActionResult> GetReviewStagesByUserId(long userId)
    {
        IReadOnlyList<ReviewStageLookupDto> response =
            await _lookupService.GetReviewStagesByUserId(userId);

        return Ok(ApiResponse<IReadOnlyList<ReviewStageLookupDto>>.Ok(response, "Success"));
    }

}
