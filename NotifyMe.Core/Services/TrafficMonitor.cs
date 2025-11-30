using System.Diagnostics;
using System.Net.NetworkInformation;
using NotifyMe.Core.Interfaces;
using NotifyMe.Models;

namespace NotifyMe.Core.Services;

public class TrafficMonitor
{
    private readonly System.Timers.Timer _updateTimer;
    private long _lastBytesReceived;
    private long _lastBytesSent;
    private DateTime _lastUpdate;
    private INetworkAdapter? _activeInterface;
    private readonly INetworkInterfaceWrapper _networkInterfaceWrapper;

    public event EventHandler<NetworkStats>? TrafficUpdated;

    public NetworkStats CurrentStats { get; private set; } = new NetworkStats();
    public int UpdateIntervalMs { get; set; } = 1000;

    public TrafficMonitor(INetworkInterfaceWrapper networkInterfaceWrapper)
    {
        _networkInterfaceWrapper = networkInterfaceWrapper;
        _updateTimer = new System.Timers.Timer();
        _updateTimer.Elapsed += (s, e) => UpdateTraffic();
        _lastUpdate = DateTime.Now;
    }

    public void Start()
    {
        SelectActiveInterface();
        InitializeCounters();
        _updateTimer.Interval = UpdateIntervalMs;
        _updateTimer.Start();
    }

    public void Stop()
    {
        _updateTimer.Stop();
    }

    private void SelectActiveInterface()
    {
        // Find the active network interface with the most traffic
        var interfaces = _networkInterfaceWrapper.GetAllNetworkInterfaces()
            .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                        ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                        ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
            .OrderByDescending(ni =>
            {
                var stats = ni.GetIPv4Statistics();
                return stats.BytesReceived + stats.BytesSent;
            })
            .FirstOrDefault();

        _activeInterface = interfaces;
    }

    private void InitializeCounters()
    {
        if (_activeInterface == null)
        {
            SelectActiveInterface();
        }

        if (_activeInterface != null)
        {
            var stats = _activeInterface.GetIPv4Statistics();
            _lastBytesReceived = stats.BytesReceived;
            _lastBytesSent = stats.BytesSent;
        }
    }

    private void UpdateTraffic()
    {
        try
        {
            if (_activeInterface == null || _activeInterface.OperationalStatus != OperationalStatus.Up)
            {
                SelectActiveInterface();
                InitializeCounters();
                return;
            }

            var stats = _activeInterface.GetIPv4Statistics();
            var now = DateTime.Now;
            var timeDiff = (now - _lastUpdate).TotalSeconds;

            if (timeDiff > 0)
            {
                var bytesReceived = stats.BytesReceived;
                var bytesSent = stats.BytesSent;

                var downloadSpeed = (bytesReceived - _lastBytesReceived) / timeDiff;
                var uploadSpeed = (bytesSent - _lastBytesSent) / timeDiff;

                CurrentStats = new NetworkStats
                {
                    Timestamp = now,
                    IsConnected = _networkInterfaceWrapper.GetIsNetworkAvailable(),
                    DownloadSpeedBytesPerSecond = Math.Max(0, downloadSpeed),
                    UploadSpeedBytesPerSecond = Math.Max(0, uploadSpeed),
                    TotalBytesReceived = bytesReceived,
                    TotalBytesSent = bytesSent,
                    InterfaceName = _activeInterface.Name,
                    IPv4Address = _activeInterface.GetIPv4Addresses().FirstOrDefault() ?? "0.0.0.0"
                };

                _lastBytesReceived = bytesReceived;
                _lastBytesSent = bytesSent;
                _lastUpdate = now;

                TrafficUpdated?.Invoke(this, CurrentStats);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating traffic: {ex.Message}");
        }
    }

    public void RefreshInterface()
    {
        SelectActiveInterface();
        InitializeCounters();
    }
}
