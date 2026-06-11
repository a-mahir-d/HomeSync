using HomeSync.WebAPI.Models;
using MassTransit;

namespace HomeSync.WebAPI.BackgroundServices;

public class SensorDataSimulatorWorker(ILogger<SensorDataSimulatorWorker> logger, IServiceProvider serviceProvider) : BackgroundService
{
    private List<Item> _items = GetDefaultItemsList();
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
            _items = GetDefaultItemsList();
            logger.LogInformation("Simülasyon durduruldu ve veriler sıfırlandı.");
        }
    }

    public bool GetStatus()
    {
        lock (_lock)
        {
            return _isSimulatorRunning;
        }
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
            if (item.Id == problemedItemId)
            {
                if (Random.Shared.Next(0, 100) < 10)
                {
                    item.CurrentDegree = 999;
                    isHardwareFault = true;
                    logger.LogCritical($"[SIMÜLATÖR] {item.Name} için DONANIMSAL ARIZA (%10 Olasılık) tetiklendi! Değer: 999°C");
                }
                else
                {
                    bool goBelow = Random.Shared.Next(0, 2) == 0;
                    if (goBelow)
                    {
                        item.CurrentDegree = Random.Shared.Next(item.MinDegree - 10, item.MinDegree);
                    }
                    else
                    {
                        item.CurrentDegree = Random.Shared.Next(item.MaxDegree + 1, item.MaxDegree + 11);
                    }
                }
            }
            else
            {
                item.CurrentDegree = Random.Shared.Next(item.MinDegree, item.MaxDegree + 1);
            }

            bool isAlarm = item.CurrentDegree < item.MinDegree || item.CurrentDegree > item.MaxDegree;
            await publishEndpoint.Publish(new SensorReadEvent
            {
                Id = item.Id,
                Name = item.Name,
                Value = item.CurrentDegree,
                IsAlarm = isAlarm || isHardwareFault,
                Timestamp = DateTime.UtcNow
            });

            logger.LogInformation($"Kuyruğa gönderildi -> {item.Name}: {item.CurrentDegree}°C");
        }
    }

    private static List<Item> GetDefaultItemsList()
    {
        return [
            new Item { Id = 1, Name = "Basement Freezer", MinDegree = -30, MaxDegree = -10, CurrentDegree = -20 },
            new Item { Id = 2, Name = "Bedroom", MinDegree = 18, MaxDegree = 22, CurrentDegree = 21 },
            new Item { Id = 3, Name = "Living Room", MinDegree = 20, MaxDegree = 24, CurrentDegree = 22 }, // Min/Max aynıydı düzelttim
            new Item { Id = 4, Name = "Kitchen Freezer", MinDegree = -18, MaxDegree = -4, CurrentDegree = -12 } // ID'yi 4 yaptım
        ];
    }
}
