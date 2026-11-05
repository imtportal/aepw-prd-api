using AEPW.Application.DTOs.BusinessRoleMapping;
using AEPW.Application.Interfaces.Services.BusinessRoleMapping;
using AEPW.Application.Interfaces.Services.Lookup;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace AEPW.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class BusinessRoleMappingController : ControllerBase
{
    private readonly IBusinessRoleMappingService _businessRoleMappingService;

    public BusinessRoleMappingController(

        IBusinessRoleMappingService businessRoleMappingService)
    {
 
        _businessRoleMappingService = businessRoleMappingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllBusinessRoleMappings([FromQuery] long? userId)
    {
        var response = await _businessRoleMappingService.GetAllBusinessRoleMappingsAsync(userId);
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> SaveBusinessRoleMapping(
        [FromBody] SaveBusinessRoleMappingRequestDto request)
    {
        var response =
            await _businessRoleMappingService.SaveBusinessRoleMappingAsync(request);

        return Ok(response);
    }

    [HttpGet("{businessRoleMappingId:long}")]
    public async Task<IActionResult> GetBusinessRoleMappingById(
        string businessRoleMappingId)
    {
        var response =
            await _businessRoleMappingService.GetBusinessRoleMappingByIdAsync(
                businessRoleMappingId);
        return Ok(response);
    }


    [HttpDelete("{businessRoleMappingId:long}")]
    public async Task<IActionResult> DeleteBusinessRoleMapping(
        [FromRoute] long businessRoleMappingId,
        [FromQuery] long updatedBy)
    {
        var response =
            await _businessRoleMappingService
                .DeleteBusinessRoleMappingAsync(
                    businessRoleMappingId,
                    updatedBy);

        return Ok(response);
    }



}