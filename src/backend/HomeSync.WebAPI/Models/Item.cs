namespace HomeSync.WebAPI.Models;

public class Item
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int MinDegree { get; set; }
    public int MaxDegree { get; set; }
    public int CurrentDegree { get; set; }
}
