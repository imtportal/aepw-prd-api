using AEPW.Application.DTOs.EmailNotificationTemplate;
using AEPW.Application.Interfaces.Services.EmailNotificationTemplate;
using Microsoft.AspNetCore.Mvc;

namespace AEPW.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class EmailNotificationTemplateController : ControllerBase
{
    private readonly IEmailNotificationTemplateService _service;

    public EmailNotificationTemplateController(
        IEmailNotificationTemplateService service)
    {
        _service = service;
    }

    /// <summary>Gets all interpolation keywords available to email templates.</summary>
    [HttpGet("interpolation-keywords")]
    public async Task<IActionResult> GetAllInterpolationKeywords(
        CancellationToken cancellationToken)
    {
        var response = await _service.GetAllInterpolationKeywordsAsync(
            cancellationToken);

        return Ok(response);
    }

    /// <summary>Gets all active email notification templates.</summary>
    [HttpGet("email-templates")]
    public async Task<IActionResult> GetAllEmailNotificationTemplates(
        CancellationToken cancellationToken)
    {
        var response = await _service.GetAllEmailNotificationTemplatesAsync(
            cancellationToken);

        return Ok(response);
    }

    /// <summary>Creates or updates an email notification template.</summary>
    [HttpPost]
    public async Task<IActionResult> SaveEmailNotificationTemplate(
        [FromBody] EmailNotificationTemplateSaveRequestDto request,
        CancellationToken cancellationToken)
    {
        var response = await _service.SaveEmailNotificationTemplateAsync(
            request,
            cancellationToken);

        return Ok(response);
    }
}
