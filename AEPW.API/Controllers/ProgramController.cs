using AEPW.Application.DTOs.Program;
using AEPW.Application.Interfaces.Services.Program;
using Microsoft.AspNetCore.Mvc;
using AEPW.Application.Contracts.Responses;
using Microsoft.AspNetCore.Authorization;
namespace AEPW.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]

public class ProgramController : ControllerBase
{
    private readonly IProgramService _programService;

    public ProgramController(
        IProgramService programService)
    {
        _programService = programService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPrograms(
        [FromQuery] GetProgramsRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _programService.GetAllProgramsAsync(
            request,
            cancellationToken);

        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpGet("{programId:long}")]
    public async Task<IActionResult> GetProgramById(
        long programId,
        CancellationToken cancellationToken)
    {
        var result = await _programService.GetProgramByIdAsync(
            programId,
            cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> InsertProgram(
        [FromBody] InsertProgramRequestDto request,
        CancellationToken cancellationToken)
    {
        await _programService.InsertProgramAsync(
            request,
            cancellationToken);

        return Ok(ApiResponse<bool>.Ok(true));
    }

    [HttpPut("{programId:long}")]
    public async Task<IActionResult> UpdateProgram(
        long programId,
        [FromBody] UpdateProgramRequestDto request,
        CancellationToken cancellationToken)
    {
        request.ProgramId = programId;

        await _programService.UpdateProgramAsync(
            request,
            cancellationToken);

        return Ok(ApiResponse<bool>.Ok(true));
    }
}