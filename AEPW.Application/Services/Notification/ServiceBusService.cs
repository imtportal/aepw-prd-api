using System.Text.Json;
using Azure.Messaging.ServiceBus;
using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.Notification;
using AEPW.Application.Interfaces.Services.Notification;
using Microsoft.Extensions.Configuration;
namespace AEPW.Application.Services.Notification

{
   public sealed class ServiceBusService : IServiceBusService
    {

        private readonly ServiceBusSender _sender;
        public ServiceBusService(
            ServiceBusClient serviceBusClient,
            IConfiguration configuration)
        {
            Console.WriteLine(configuration["ServiceBus:QueueName"]);
            Console.WriteLine(configuration["ServiceBus:ConnectionString"]);

             
            _sender = serviceBusClient.CreateSender(
                configuration["ServiceBus:QueueName"]);
        }


        public async Task<ApiResponse<bool>> PublishNotificationAsync(
        NotificationMessageDto notification)
        {
            try
            {
                 
                string messageBody = JsonSerializer.Serialize(notification);

                ServiceBusMessage message = new(messageBody);
                Console.WriteLine("Before Send");
                await _sender.SendMessageAsync(message);
                Console.WriteLine("After Send");

                return ApiResponse<bool>.Ok(
                    true,
                    "Notification queued successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);

                return ApiResponse<bool>.Fail(
                    ex.Message);
            }
        }

    }
}
