using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.FundingRecipient;
using AEPW.Application.DTOs.FundingRecipient.Contact;
using AEPW.Application.Interfaces.Services.FundingRecipient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
namespace AEPW.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class FundingRecipientController : ControllerBase
{
    private readonly IFundingRecipientService _service;

    public FundingRecipientController(
        IFundingRecipientService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] GetFundingRecipientsRequestDto request,
        CancellationToken cancellationToken)
    {
        var result =
            await _service.GetAllAsync(request, cancellationToken);

        return Ok(ApiResponse<object>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
    [FromBody] CreateFundingRecipientRequestDto request,
    CancellationToken cancellationToken)
    {
        var fundingRecipientId =
            await _service.CreateAsync(
                request,
                cancellationToken);

        return Ok(
            ApiResponse<object>.Ok(
                fundingRecipientId,
                "Funding recipient created successfully."));
    }
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(
    long id,
    [FromBody] UpdateFundingRecipientRequestDto request,
    CancellationToken cancellationToken)
    {
        request.FundingRecipientId = id;

        await _service.UpdateAsync(
            request,
            cancellationToken);

        return Ok(
            ApiResponse<object>.Ok(
                true,
                "Funding recipient updated successfully."));
    }
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(
    long id,
    CancellationToken cancellationToken)
    {
        var result =
            await _service.GetByIdAsync(
                id,
                cancellationToken);

        if (result is null)
        {
            return NotFound(
                ApiResponse<object>.Fail(
                    "Funding recipient not found."));
        }

        return Ok(
            ApiResponse<object>.Ok(result));
    }

}