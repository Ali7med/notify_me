using System.Net.NetworkInformation;

namespace NotifyMe.Core.Interfaces;

public interface INetworkAdapter
{
    string Name { get; }
    string Description { get; }
    string Id { get; }
    OperationalStatus OperationalStatus { get; }
    NetworkInterfaceType NetworkInterfaceType { get; }
    INetworkStatistics GetIPv4Statistics();
    IEnumerable<string> GetIPv4Addresses();
}
