using HomeSync.WebAPI.Models;
using MassTransit;

namespace HomeSync.WebAPI.BackgroundServices;

public class SensorDataSimulatorWorker(ILogger<SensorDataSimulatorWorker> logger, IServiceProvider serviceProvider) : BackgroundService
{
    private readonly List<Item> _items =
    [
        new Item { Id = 1, Name = "Basement Freezer", MinDegree = -30, MaxDegree = -10 },
        new Item { Id = 2, Name = "Bedroom", MinDegree = 18, MaxDegree = 22 },
        new Item { Id = 3, Name = "Living Room", MinDegree = 20, MaxDegree = 24 },
        new Item { Id = 4, Name = "Kitchen Freezer", MinDegree = -18, MaxDegree = -4 }
    ];
    private bool _isSimulatorRunning = false;
    private readonly Lock _lock = new();

    public void RunSimulator()
    {
        lock (_lock)
        {
            _isSimulatorRunning = true;
            logger.LogInformation("Simülasyon çalıştırıldı.");
        }
    }

    public void StopSimulator()
    {
        lock (_lock)
        {
            _isSimulatorRunning = false;
            logger.LogInformation("Simülasyon durduruldu.");
        }
    }

    public bool GetStatus()
    {
        lock (_lock)
        {
            return _isSimulatorRunning;
        }
    }

    public List<Item> GetItems()
    {
        return _items;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Sensor Data Simulator Worker başlatıldı.");

        while (!stoppingToken.IsCancellationRequested)
        {
            bool isRunning;
            lock (_lock) { isRunning = _isSimulatorRunning; }

            if (!isRunning)
            {
                await Task.Delay(1000, stoppingToken);
                continue;
            }

            try
            {
                await RunSimulationCycleAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Simülasyon döngüsünde bir hata oluştu.");
            }

            await Task.Delay(3000, stoppingToken);
        }
    }

    private async Task RunSimulationCycleAsync()
    {
        var allIds = _items.Select(i => i.Id).ToList();
        var problemedItemId = allIds[Random.Shared.Next(allIds.Count)];

        using var scope = serviceProvider.CreateScope();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        foreach (var item in _items)
        {
            bool isHardwareFault = false;
            int newDegree;
            if (item.Id == problemedItemId)
            {
                if (Random.Shared.Next(0, 100) < 10)
                {
                    newDegree = 999;
                    isHardwareFault = true;
                    logger.LogCritical($"[SIMÜLATÖR] {item.Name} için DONANIMSAL ARIZA (%10 Olasılık) tetiklendi! Değer: 999°C");
                }
                else
                {
                    bool goBelow = Random.Shared.Next(0, 2) == 0;
                    if (goBelow)
                    {
                        newDegree = Random.Shared.Next(item.MinDegree - 10, item.MinDegree);
                    }
                    else
                    {
                        newDegree = Random.Shared.Next(item.MaxDegree + 1, item.MaxDegree + 11);
                    }
                }
            }
            else
            {
                newDegree = Random.Shared.Next(item.MinDegree, item.MaxDegree + 1);
            }

            bool isAlarm = newDegree < item.MinDegree || newDegree > item.MaxDegree;
            await publishEndpoint.Publish(new SensorReadEvent
            {
                Id = item.Id,
                Value = newDegree,
                IsAlarm = isAlarm || isHardwareFault,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
