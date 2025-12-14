namespace NotifyMe.Models;

/// <summary>
/// معلومات استهلاك الشبكة لكل تطبيق
/// </summary>
public class ProcessNetworkInfo
{
    /// <summary>
    /// معرف العملية (Process ID)
    /// </summary>
    public int ProcessId { get; set; }

    /// <summary>
    /// اسم التطبيق
    /// </summary>
    public string ProcessName { get; set; } = string.Empty;

    /// <summary>
    /// المسار الكامل للتطبيق
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// أيقونة التطبيق (Base64 أو مسار)
    /// </summary>
    public string? IconPath { get; set; }

    /// <summary>
    /// إجمالي البيانات المرسلة (Bytes)
    /// </summary>
    public long TotalBytesSent { get; set; }

    /// <summary>
    /// إجمالي البيانات المستقبلة (Bytes)
    /// </summary>
    public long TotalBytesReceived { get; set; }

    /// <summary>
    /// سرعة الإرسال الحالية (Bytes/sec)
    /// </summary>
    public long CurrentSendRate { get; set; }

    /// <summary>
    /// سرعة الاستقبال الحالية (Bytes/sec)
    /// </summary>
    public long CurrentReceiveRate { get; set; }

    /// <summary>
    /// عدد الاتصالات النشطة
    /// </summary>
    public int ActiveConnections { get; set; }

    /// <summary>
    /// آخر وقت نشاط
    /// </summary>
    public DateTime LastActivity { get; set; }

    /// <summary>
    /// تصنيف التطبيق (Browser, Game, System, etc.)
    /// </summary>
    public string Category { get; set; } = "Unknown";

    /// <summary>
    /// علامات مخصصة من المستخدم
    /// </summary>
    public List<string> Tags { get; set; } = new();

    // Computed Properties
    
    /// <summary>
    /// إجمالي الاستهلاك (Upload + Download)
    /// </summary>
    public long TotalBytes => TotalBytesSent + TotalBytesReceived;

    /// <summary>
    /// إجمالي الاستهلاك بصيغة قابلة للقراءة
    /// </summary>
    public string TotalBytesFormatted => FormatBytes(TotalBytes);

    /// <summary>
    /// سرعة الإرسال بصيغة قابلة للقراءة
    /// </summary>
    public string SendRateFormatted => FormatBytes(CurrentSendRate) + "/s";

    /// <summary>
    /// سرعة الاستقبال بصيغة قابلة للقراءة
    /// </summary>
    public string ReceiveRateFormatted => FormatBytes(CurrentReceiveRate) + "/s";

    /// <summary>
    /// تنسيق البايتات لصيغة قابلة للقراءة
    /// </summary>
    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
