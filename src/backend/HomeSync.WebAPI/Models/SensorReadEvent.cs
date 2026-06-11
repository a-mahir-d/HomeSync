namespace HomeSync.WebAPI.Models;

public record SensorReadEvent
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Value { get; init; }
    public bool IsAlarm { get; init; }
    public DateTime Timestamp { get; init; }
}
