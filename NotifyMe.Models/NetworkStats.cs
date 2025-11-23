namespace NotifyMe.Models;

public class NetworkStats
{
    public DateTime Timestamp { get; set; }
    public bool IsConnected { get; set; }
    public double DownloadSpeedBytesPerSecond { get; set; }
    public double UploadSpeedBytesPerSecond { get; set; }
    public long TotalBytesReceived { get; set; }
    public long TotalBytesSent { get; set; }

    public string DownloadSpeedFormatted => FormatSpeed(DownloadSpeedBytesPerSecond);
    public string UploadSpeedFormatted => FormatSpeed(UploadSpeedBytesPerSecond);

    private static string FormatSpeed(double bytesPerSecond)
    {
        if (bytesPerSecond < 1024)
            return $"{bytesPerSecond:F0} B/s";
        if (bytesPerSecond < 1024 * 1024)
            return $"{bytesPerSecond / 1024:F1} KB/s";
        if (bytesPerSecond < 1024 * 1024 * 1024)
            return $"{bytesPerSecond / (1024 * 1024):F1} MB/s";
        return $"{bytesPerSecond / (1024 * 1024 * 1024):F2} GB/s";
    }

    public static string FormatBytes(long bytes)
    {
        if (bytes < 1024)
            return $"{bytes} B";
        if (bytes < 1024 * 1024)
            return $"{bytes / 1024.0:F1} KB";
        if (bytes < 1024L * 1024 * 1024)
            return $"{bytes / (1024.0 * 1024):F1} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
    }
}
