using AEPW.API.Extensions;
using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.Authorization;
using AEPW.Application.Interfaces.Services.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace AEPW.API.Controllers;

[Authorize]
[ApiController]
[Route("api/role-access")]
public sealed class RoleAccessController : ControllerBase
{
    private readonly IRoleAccessService _roleAccessService;
    private readonly ILogger<RoleAccessController> _logger;

    public RoleAccessController(
        IRoleAccessService roleAccessService,
        ILogger<RoleAccessController> logger)
    {
        _roleAccessService = roleAccessService;
        _logger = logger;
    }

    /// <summary>
    /// Get role-page access matrix for all system roles and pages
    /// </summary>
    [HttpGet("pages")]
    public async Task<IActionResult> GetRolePageAccess(
        CancellationToken cancellationToken = default)
    {
        var response = await _roleAccessService.GetRolePageAccessAsync(cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Get role-page action access matrix for a specific page
    /// </summary>
    [HttpGet("pages/{pageStructureId:long}/actions")]
    public async Task<IActionResult> GetRolePageActionAccess(
        long pageStructureId,
        CancellationToken cancellationToken = default)
    {
        var response = await _roleAccessService.GetRolePageActionAccessAsync(
            pageStructureId,
            cancellationToken);

        return Ok(response);
    }

    /// <summary>
    /// Save role-page access for a specific role
    /// </summary>
    [HttpPost("pages")]
    public async Task<IActionResult> SaveRolePageAccess(
        [FromBody] SaveRolePageAccessRequestDto request,
        CancellationToken cancellationToken = default)
    {

        var response = await _roleAccessService.SaveRolePageAccessAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Save role-page action access for a specific role and page
    /// </summary>
    [HttpPost("pages/actions")]
    public async Task<IActionResult> SaveRolePageActionAccess(
        [FromBody] SaveRolePageActionAccessRequestDto request,
        CancellationToken cancellationToken = default)
    {

        var response = await _roleAccessService.SaveRolePageActionAccessAsync(request, cancellationToken);
        return Ok(response);
    }
}
