using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.Notification;

namespace AEPW.Application.Interfaces.Services.Notification
{
    public interface IServiceBusService
    {
        Task<ApiResponse<bool>> PublishNotificationAsync(NotificationMessageDto notification);
    }
}
