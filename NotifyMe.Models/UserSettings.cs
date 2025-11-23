namespace NotifyMe.Models;

public class UserSettings
{
    public double Opacity { get; set; } = 0.8;
    public string Theme { get; set; } = "Glass"; // "Glass" or "Classic"
    public bool IsTransparent { get; set; } = true;
    public string PingHost { get; set; } = "8.8.8.8";
}
