using HomeSync.WebAPI.BackgroundServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeSync.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SimulatorController(SensorDataSimulatorWorker simulator) : ControllerBase
{
    [HttpGet("items")]
    public IActionResult GetItems()
    {
        var data = simulator.GetItems();
        return Ok(data);
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        var status = simulator.GetStatus();
        return Ok(status);
    }

    [HttpPost("start")]
    public IActionResult Start()
    {
        simulator.RunSimulator();
        return Ok();
    }

    [HttpPost("stop")]
    public IActionResult Stop()
    {
        simulator.StopSimulator();
        return Ok();
    }
}
