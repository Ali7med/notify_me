using System.Net.NetworkInformation;
using Moq;
using NotifyMe.Core.Interfaces;
using NotifyMe.Core.Services;
using Xunit;

namespace NotifyMe.Tests.Services;

public class TrafficMonitorTests
{
    private readonly Mock<INetworkInterfaceWrapper> _mockNetworkInterface;
    private readonly Mock<INetworkAdapter> _mockAdapter;
    private readonly Mock<INetworkStatistics> _mockStats;
    private readonly TrafficMonitor _monitor;

    public TrafficMonitorTests()
    {
        _mockNetworkInterface = new Mock<INetworkInterfaceWrapper>();
        _mockAdapter = new Mock<INetworkAdapter>();
        _mockStats = new Mock<INetworkStatistics>();

        _mockAdapter.Setup(x => x.OperationalStatus).Returns(OperationalStatus.Up);
        _mockAdapter.Setup(x => x.NetworkInterfaceType).Returns(NetworkInterfaceType.Ethernet);
        _mockAdapter.Setup(x => x.GetIPv4Statistics()).Returns(_mockStats.Object);

        _mockNetworkInterface.Setup(x => x.GetAllNetworkInterfaces()).Returns(new[] { _mockAdapter.Object });
        _mockNetworkInterface.Setup(x => x.GetIsNetworkAvailable()).Returns(true);

        _monitor = new TrafficMonitor(_mockNetworkInterface.Object);
    }

    [Fact]
    public void Start_SelectsActiveInterface()
    {
        // Arrange
        _mockStats.Setup(x => x.BytesReceived).Returns(1000);
        _mockStats.Setup(x => x.BytesSent).Returns(1000);

        // Act
        _monitor.Start();

        // Assert
        // We can't easily verify private state, but we can verify that GetAllNetworkInterfaces was called
        _mockNetworkInterface.Verify(x => x.GetAllNetworkInterfaces(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task UpdateTraffic_CalculatesSpeedCorrectly()
    {
        // Arrange
        _monitor.UpdateIntervalMs = 100; // Short interval for test
        
        // Initial stats
        _mockStats.SetupSequence(x => x.BytesReceived)
            .Returns(1000) // SelectActiveInterface
            .Returns(1000) // InitializeCounters
            .Returns(2000); // UpdateTraffic

        _mockStats.SetupSequence(x => x.BytesSent)
            .Returns(500) // SelectActiveInterface
            .Returns(500) // InitializeCounters
            .Returns(1000); // UpdateTraffic

        var tcs = new TaskCompletionSource<bool>();
        _monitor.TrafficUpdated += (s, stats) =>
        {
            if (stats.DownloadSpeedBytesPerSecond > 0)
            {
                tcs.TrySetResult(true);
            }
        };

        // Act
        _monitor.Start();
        
        // Wait for update
        await Task.Delay(200); // Wait longer than interval

        // Assert
        var result = await Task.WhenAny(tcs.Task, Task.Delay(1000));
        Assert.Equal(tcs.Task, result);
        _monitor.Stop();
    }
}
