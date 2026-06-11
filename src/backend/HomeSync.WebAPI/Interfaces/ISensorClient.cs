using HomeSync.WebAPI.Models;

namespace HomeSync.WebAPI.Interfaces;

public interface ISensorClient
{
    Task ReceiveSensorData(SensorReadEvent sensorData);
}
