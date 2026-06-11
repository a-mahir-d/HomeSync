using HomeSync.WebAPI.Hubs;
using HomeSync.WebAPI.Models;
using MassTransit;
using Microsoft.AspNetCore.SignalR;

namespace HomeSync.WebAPI.Consumers;

public class SensorDataConsumer(IHubContext<SensorHub> hubContext) : IConsumer<SensorReadEvent>
{
    public async Task Consume(ConsumeContext<SensorReadEvent> context)
    {
        var message = context.Message;
        if (message.IsAlarm)
        {
            if (message.Value == 999)
            {
                throw new InvalidOperationException("HARDWARE_ERROR");
            }

            await hubContext.Clients.All.SendAsync("ReceiveSensorAlarm", message);
        }
        else
        {
            await hubContext.Clients.All.SendAsync("ReceiveSensorData", message);
        }
    }
}
