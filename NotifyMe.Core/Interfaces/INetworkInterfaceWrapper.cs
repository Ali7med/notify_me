using System.Net.NetworkInformation;

namespace NotifyMe.Core.Interfaces;

public interface INetworkInterfaceWrapper
{
    bool GetIsNetworkAvailable();
    INetworkAdapter[] GetAllNetworkInterfaces();
}
