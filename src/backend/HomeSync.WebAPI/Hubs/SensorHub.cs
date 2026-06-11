using HomeSync.WebAPI.BackgroundServices;
using HomeSync.WebAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HomeSync.WebAPI.Hubs;

[Authorize]
public sealed class SensorHub(SensorDataSimulatorWorker simulator) : Hub<ISensorClient>
{
    private static int _activeConnections = 0;

    public override async Task OnConnectedAsync()
    {
        Interlocked.Increment(ref _activeConnections);
        Console.WriteLine($"[SignalR] Client connected. Total: {_activeConnections}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Interlocked.Decrement(ref _activeConnections);
        Console.WriteLine($"[SignalR] Client disconnected. Total: {_activeConnections}");

        if (_activeConnections <= 0)
        {
            _activeConnections = 0;
            simulator.StopSimulator();
            Console.WriteLine("[SignalR Auto-Stop] No active clients left. Simulator auto-stopped to save Neon/Render resources.");
        }

        await base.OnDisconnectedAsync(exception);
    }
}
