using System.Net.NetworkInformation;
using NotifyMe.Core.Interfaces;

namespace NotifyMe.Core.Services;

public class NetworkStatisticsWrapper : INetworkStatistics
{
    private readonly IPv4InterfaceStatistics _stats;

    public NetworkStatisticsWrapper(IPv4InterfaceStatistics stats)
    {
        _stats = stats;
    }

    public long BytesReceived => _stats.BytesReceived;
    public long BytesSent => _stats.BytesSent;
}
