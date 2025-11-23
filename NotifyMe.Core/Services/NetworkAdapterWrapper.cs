using System.Net.NetworkInformation;
using NotifyMe.Core.Interfaces;

namespace NotifyMe.Core.Services;

public class NetworkAdapterWrapper : INetworkAdapter
{
    private readonly NetworkInterface _networkInterface;

    public NetworkAdapterWrapper(NetworkInterface networkInterface)
    {
        _networkInterface = networkInterface;
    }

    public string Name => _networkInterface.Name;
    public string Description => _networkInterface.Description;
    public string Id => _networkInterface.Id;
    public OperationalStatus OperationalStatus => _networkInterface.OperationalStatus;
    public NetworkInterfaceType NetworkInterfaceType => _networkInterface.NetworkInterfaceType;

    public INetworkStatistics GetIPv4Statistics()
    {
        return new NetworkStatisticsWrapper(_networkInterface.GetIPv4Statistics());
    }
}
