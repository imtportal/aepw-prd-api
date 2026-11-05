using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.Notification;
using AEPW.Application.Interfaces.Services.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AEPW.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public sealed class NotificationController : ControllerBase
    {
        private readonly IServiceBusService _serviceBusService;

        public NotificationController(IServiceBusService serviceBusService)
        {
            _serviceBusService = serviceBusService;
        }

        [AllowAnonymous]
        [HttpPost("publish")]
        public async Task<IActionResult> PublishNotification(
            [FromBody] NotificationMessageDto request)
        {
            ApiResponse<bool> result =
                await _serviceBusService.PublishNotificationAsync(request);

            return Ok(result);
        }
    } 
}