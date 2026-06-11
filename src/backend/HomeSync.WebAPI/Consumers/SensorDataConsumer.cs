using HomeSync.WebAPI.Hubs;
using HomeSync.WebAPI.Models;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace HomeSync.WebAPI.Consumers;

public class SensorDataConsumer(ILogger<SensorDataConsumer> logger, IHubContext<SensorHub> hubContext) : IConsumer<SensorReadEvent>
{
    public async Task Consume(ConsumeContext<SensorReadEvent> context)
    {
        var message = context.Message;
        if (message.IsAlarm)
        {
            logger.LogWarning($"[ALARM] {message.Name} kritik seviyede! Değer: {message.Value}°C | Zaman: {message.Timestamp}");
            if (message.Value == 999)
            {
                throw new InvalidOperationException("HARDWARE_ERROR");
            }

            await hubContext.Clients.All.SendAsync("ReceiveSensorAlarm", message);
        }
        else
        {
            logger.LogInformation($"[NORMAL] {message.Name}: {message.Value}°C");
            await hubContext.Clients.All.SendAsync("ReceiveSensorData", message);
        }
    }
}
