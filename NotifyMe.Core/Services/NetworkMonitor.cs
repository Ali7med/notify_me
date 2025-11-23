using System.Net.NetworkInformation;
using NotifyMe.Core.Interfaces;
using NotifyMe.Models;

namespace NotifyMe.Core.Services;

public class NetworkMonitor
{
    private readonly System.Timers.Timer _checkTimer;
    private bool _lastConnectionState = true;
    private readonly string[] _testHosts = { "8.8.8.8", "1.1.1.1", "208.67.222.222" }; // Google, Cloudflare, OpenDNS
    private readonly INetworkInterfaceWrapper _networkInterfaceWrapper;
    private readonly IPingWrapper _pingWrapper;

    public event EventHandler<ConnectionEvent>? ConnectionStateChanged;
    public event EventHandler<NetworkStats>? StatsUpdated;
    public event EventHandler<long>? LatencyChanged;

    public bool IsConnected { get; private set; } = true;
    public int CheckIntervalSeconds { get; set; } = 5;
    public string PingHost { get; set; } = "8.8.8.8";

    public NetworkMonitor(INetworkInterfaceWrapper networkInterfaceWrapper, IPingWrapper pingWrapper)
    {
        _networkInterfaceWrapper = networkInterfaceWrapper;
        _pingWrapper = pingWrapper;
        _checkTimer = new System.Timers.Timer();
        _checkTimer.Elapsed += async (s, e) => await CheckConnectionAsync();
    }

    public void Start()
    {
        _checkTimer.Interval = CheckIntervalSeconds * 1000;
        _checkTimer.Start();
        Task.Run(async () => await CheckConnectionAsync());
    }

    public void Stop()
    {
        _checkTimer.Stop();
    }

    private async Task<bool> CheckConnectionAsync()
    {
        try
        {
            // Check if any network interface is up
            if (!_networkInterfaceWrapper.GetIsNetworkAvailable())
            {
                UpdateConnectionState(false);
                return false;
            }

            // Ping multiple DNS servers
            var pingTasks = _testHosts.Select(async host =>
            {
                return await _pingWrapper.SendPingAsync(host, 3000);
            });

            var results = await Task.WhenAll(pingTasks);
            bool isConnected = results.Any(r => r);

            // Measure latency to the configured PingHost
            if (isConnected)
            {
                try
                {
                    using var ping = new Ping();
                    var reply = await ping.SendPingAsync(PingHost, 3000);
                    if (reply.Status == IPStatus.Success)
                    {
                        LatencyChanged?.Invoke(this, reply.RoundtripTime);
                    }
                }
                catch
                {
                    // Ignore ping errors for latency check
                }
            }

            UpdateConnectionState(isConnected);
            return isConnected;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking connection: {ex.Message}");
            UpdateConnectionState(false);
            return false;
        }
    }

    private void UpdateConnectionState(bool isConnected)
    {
        IsConnected = isConnected;

        if (_lastConnectionState != isConnected)
        {
            _lastConnectionState = isConnected;

            var connectionEvent = new ConnectionEvent
            {
                Timestamp = DateTime.Now,
                EventType = isConnected ? ConnectionEventType.Connected : ConnectionEventType.Disconnected,
                Message = isConnected
                    ? "Internet connection restored"
                    : "Internet connection lost"
            };

            ConnectionStateChanged?.Invoke(this, connectionEvent);
        }
    }

    public async Task<bool> CheckNowAsync()
    {
        return await CheckConnectionAsync();
    }
}
