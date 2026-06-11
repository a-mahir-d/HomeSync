using System.ComponentModel.DataAnnotations;

namespace HomeSync.WebAPI.Models.Settings;

public class ClientSettings
{
    [Required]
    public required string BaseUrl { get; set; }
}

