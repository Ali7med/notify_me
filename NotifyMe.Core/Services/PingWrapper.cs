using System.Net.NetworkInformation;
using NotifyMe.Core.Interfaces;

namespace NotifyMe.Core.Services;

public class PingWrapper : IPingWrapper
{
    public async Task<bool> SendPingAsync(string hostNameOrAddress, int timeout)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(hostNameOrAddress, timeout);
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }
}
