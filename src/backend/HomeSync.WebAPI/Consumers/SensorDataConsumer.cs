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
            if (message.Value == 999)
            {
                logger.LogCritical($"[HARDWARE] Kritik donanımsal sensör arızası! Sensör ID: {message.Id}");
                await hubContext.Clients.All.SendAsync("ReceiveHardwareError", $"SENSOR_ERROR_ON_{message.Id}");
            }

            await hubContext.Clients.All.SendAsync("ReceiveSensorAlarm", message);
        }
        else
        {
            await hubContext.Clients.All.SendAsync("ReceiveSensorData", message);
        }
    }
}
