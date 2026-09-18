namespace PowerGrid.Api.Models;

public class Device
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Voltage { get; set; }
    public double Current { get; set; }
    public double Temperature { get; set; }
    public string Status { get; set; } = "NORMAL";
    public bool AlarmAcknowledged { get; set; }
}