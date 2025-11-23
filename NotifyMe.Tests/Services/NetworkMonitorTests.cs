using Moq;
using NotifyMe.Core.Interfaces;
using NotifyMe.Core.Services;
using NotifyMe.Models;
using Xunit;

namespace NotifyMe.Tests.Services;

public class NetworkMonitorTests
{
    private readonly Mock<INetworkInterfaceWrapper> _mockNetworkInterface;
    private readonly Mock<IPingWrapper> _mockPing;
    private readonly NetworkMonitor _monitor;

    public NetworkMonitorTests()
    {
        _mockNetworkInterface = new Mock<INetworkInterfaceWrapper>();
        _mockPing = new Mock<IPingWrapper>();
        _monitor = new NetworkMonitor(_mockNetworkInterface.Object, _mockPing.Object);
    }

    [Fact]
    public async Task CheckNowAsync_NetworkUnavailable_ReturnsFalse()
    {
        // Arrange
        _mockNetworkInterface.Setup(x => x.GetIsNetworkAvailable()).Returns(false);

        // Act
        var result = await _monitor.CheckNowAsync();

        // Assert
        Assert.False(result);
        Assert.False(_monitor.IsConnected);
    }

    [Fact]
    public async Task CheckNowAsync_NetworkAvailable_PingFails_ReturnsFalse()
    {
        // Arrange
        _mockNetworkInterface.Setup(x => x.GetIsNetworkAvailable()).Returns(true);
        _mockPing.Setup(x => x.SendPingAsync(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(false);

        // Act
        var result = await _monitor.CheckNowAsync();

        // Assert
        Assert.False(result);
        Assert.False(_monitor.IsConnected);
    }

    [Fact]
    public async Task CheckNowAsync_NetworkAvailable_PingSucceeds_ReturnsTrue()
    {
        // Arrange
        _mockNetworkInterface.Setup(x => x.GetIsNetworkAvailable()).Returns(true);
        _mockPing.Setup(x => x.SendPingAsync(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(true);

        // Act
        var result = await _monitor.CheckNowAsync();

        // Assert
        Assert.True(result);
        Assert.True(_monitor.IsConnected);
    }

    [Fact]
    public async Task CheckNowAsync_StateChange_RaisesEvent()
    {
        // Arrange
        _mockNetworkInterface.Setup(x => x.GetIsNetworkAvailable()).Returns(true);
        _mockPing.Setup(x => x.SendPingAsync(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(true);
        
        // Initial check to set state to Connected
        await _monitor.CheckNowAsync();

        // Now simulate disconnect
        _mockNetworkInterface.Setup(x => x.GetIsNetworkAvailable()).Returns(false);
        
        var eventRaised = false;
        _monitor.ConnectionStateChanged += (s, e) =>
        {
            eventRaised = true;
            Assert.Equal(ConnectionEventType.Disconnected, e.EventType);
        };

        // Act
        await _monitor.CheckNowAsync();

        // Assert
        Assert.True(eventRaised);
    }
}
