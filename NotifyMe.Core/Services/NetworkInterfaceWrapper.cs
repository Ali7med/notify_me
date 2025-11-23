using System.Net.NetworkInformation;
using NotifyMe.Core.Interfaces;

namespace NotifyMe.Core.Services;

public class NetworkInterfaceWrapper : INetworkInterfaceWrapper
{
    public bool GetIsNetworkAvailable()
    {
        return NetworkInterface.GetIsNetworkAvailable();
    }

    public INetworkAdapter[] GetAllNetworkInterfaces()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Select(ni => new NetworkAdapterWrapper(ni))
            .Cast<INetworkAdapter>()
            .ToArray();
    }
}
