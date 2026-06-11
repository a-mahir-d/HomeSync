using System.ComponentModel.DataAnnotations;

namespace HomeSync.WebAPI.Models.Settings;

public class RabbitMQSettings
{
    [Required]
    public required string ConnectionString { get; set; }
}
