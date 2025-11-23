using System.Net.NetworkInformation;

namespace NotifyMe.Core.Interfaces;

public interface IPingWrapper
{
    Task<bool> SendPingAsync(string hostNameOrAddress, int timeout);
}
