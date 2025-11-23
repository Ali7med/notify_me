namespace NotifyMe.Core.Interfaces;

public interface INetworkStatistics
{
    long BytesReceived { get; }
    long BytesSent { get; }
}
