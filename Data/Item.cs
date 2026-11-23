namespace TemperatureBlazorApp.Data;

public class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double TemperatureCelsius { get; set; }
}
