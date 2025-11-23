namespace NotifyMe.Models;

public class ConnectionEvent
{
    public DateTime Timestamp { get; set; }
    public ConnectionEventType EventType { get; set; }
    public string Message { get; set; } = string.Empty;
}

public enum ConnectionEventType
{
    Connected,
    Disconnected,
    Checking
}
