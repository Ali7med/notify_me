namespace NotifyMe.Models;

public class NetworkLog
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsConnected { get; set; }
    public double DownloadSpeedBps { get; set; }
    public double UploadSpeedBps { get; set; }
    public long Latency { get; set; } // -1 for timeout/error
}
