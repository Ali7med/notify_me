using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace NotifyMe.Core.Services;

public class ProcessMonitorService
{
    private readonly ConcurrentDictionary<int, ProcessNetworkStats> _processStats = new();
    private readonly Timer _monitorTimer;
    private bool _isMonitoring;

    public event EventHandler<Dictionary<int, ProcessNetworkStats>>? StatsUpdated;

    public ProcessMonitorService()
    {
        _monitorTimer = new Timer(MonitorProcesses, null, Timeout.Infinite, Timeout.Infinite);
    }

    public void Start()
    {
        if (_isMonitoring) return;
        _isMonitoring = true;
        _monitorTimer.Change(0, 3000); // Update every 3 seconds
    }

    public void Stop()
    {
        _isMonitoring = false;
        _monitorTimer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    private void MonitorProcesses(object? state)
    {
        try
        {
            var tcpConnections = GetAllTcpConnections();
            var processGroups = tcpConnections.GroupBy(c => c.ProcessId);

            foreach (var group in processGroups)
            {
                try
                {
                    var processId = group.Key;
                    if (processId == 0) continue;

                    var process = Process.GetProcessById(processId);
                    
                    if (!_processStats.ContainsKey(processId))
                    {
                        _processStats[processId] = new ProcessNetworkStats
                        {
                            ProcessId = processId,
                            ProcessName = process.ProcessName,
                            ExecutablePath = GetProcessPath(process),
                            StartTime = process.StartTime,
                            ConnectionCount = group.Count(),
                            BytesSent = 0,
                            BytesReceived = 0
                        };
                    }
                    else
                    {
                        _processStats[processId].ConnectionCount = group.Count();
                        _processStats[processId].LastActivity = DateTime.Now;
                        
                        // Simulate data transfer (in real implementation, track actual bytes)
                        _processStats[processId].BytesSent += group.Count() * 1024; // Simulate
                        _processStats[processId].BytesReceived += group.Count() * 2048; // Simulate
                    }
                }
                catch
                {
                    // Skip processes we can't access
                }
            }

            // Clean up old processes (no connections in last 30 seconds)
            var staleProcesses = _processStats.Where(p => 
                (DateTime.Now - p.Value.LastActivity).TotalSeconds > 30 &&
                !tcpConnections.Any(c => c.ProcessId == p.Key)).Select(p => p.Key).ToList();

            foreach (var pid in staleProcesses)
            {
                _processStats.TryRemove(pid, out _);
            }

            StatsUpdated?.Invoke(this, new Dictionary<int, ProcessNetworkStats>(_processStats));
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ProcessMonitor error: {ex.Message}");
        }
    }

    private List<TcpProcessRecord> GetAllTcpConnections()
    {
        var connections = new List<TcpProcessRecord>();

        try
        {
            // Get IPv4 connections
            connections.AddRange(GetTcpTable(2)); // AF_INET
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetAllTcpConnections error: {ex.Message}");
        }

        return connections;
    }

    private List<TcpProcessRecord> GetTcpTable(int ipVersion)
    {
        var connections = new List<TcpProcessRecord>();
        
        try
        {
            int buffSize = 0;
            GetExtendedTcpTable(IntPtr.Zero, ref buffSize, true, ipVersion, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL, 0);

            IntPtr tcpTablePtr = Marshal.AllocHGlobal(buffSize);

            try
            {
                if (GetExtendedTcpTable(tcpTablePtr, ref buffSize, true, ipVersion, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL, 0) == 0)
                {
                    MIB_TCPTABLE_OWNER_PID tcpTable = Marshal.PtrToStructure<MIB_TCPTABLE_OWNER_PID>(tcpTablePtr);
                    IntPtr rowPtr = (IntPtr)((long)tcpTablePtr + Marshal.SizeOf(tcpTable.dwNumEntries));

                    for (int i = 0; i < tcpTable.dwNumEntries; i++)
                    {
                        MIB_TCPROW_OWNER_PID tcpRow = Marshal.PtrToStructure<MIB_TCPROW_OWNER_PID>(rowPtr);
                        
                        connections.Add(new TcpProcessRecord
                        {
                            ProcessId = tcpRow.owningPid,
                            LocalPort = (ushort)((tcpRow.localPort >> 8) | (tcpRow.localPort << 8) & 0xFF00),
                            State = (TcpState)tcpRow.state
                        });

                        rowPtr = (IntPtr)((long)rowPtr + Marshal.SizeOf(typeof(MIB_TCPROW_OWNER_PID)));
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(tcpTablePtr);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetTcpTable error: {ex.Message}");
        }

        return connections;
    }

    private string GetProcessPath(Process process)
    {
        try
        {
            return process.MainModule?.FileName ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public Dictionary<int, ProcessNetworkStats> GetCurrentStats()
    {
        return new Dictionary<int, ProcessNetworkStats>(_processStats);
    }

    // P/Invoke declarations
    [DllImport("iphlpapi.dll", SetLastError = true)]
    private static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int dwOutBufLen, bool sort, int ipVersion, TCP_TABLE_CLASS tblClass, int reserved);

    [StructLayout(LayoutKind.Sequential)]
    private struct MIB_TCPTABLE_OWNER_PID
    {
        public uint dwNumEntries;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MIB_TCPROW_OWNER_PID
    {
        public uint state;
        public uint localAddr;
        public uint localPort;
        public uint remoteAddr;
        public uint remotePort;
        public int owningPid;
    }

    private enum TCP_TABLE_CLASS
    {
        TCP_TABLE_BASIC_LISTENER,
        TCP_TABLE_BASIC_CONNECTIONS,
        TCP_TABLE_BASIC_ALL,
        TCP_TABLE_OWNER_PID_LISTENER,
        TCP_TABLE_OWNER_PID_CONNECTIONS,
        TCP_TABLE_OWNER_PID_ALL,
        TCP_TABLE_OWNER_MODULE_LISTENER,
        TCP_TABLE_OWNER_MODULE_CONNECTIONS,
        TCP_TABLE_OWNER_MODULE_ALL
    }
}

public class TcpProcessRecord
{
    public int ProcessId { get; set; }
    public ushort LocalPort { get; set; }
    public TcpState State { get; set; }
}

public class ProcessNetworkStats
{
    public int ProcessId { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public string ExecutablePath { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime LastActivity { get; set; } = DateTime.Now;
    public int ConnectionCount { get; set; }
    public long BytesSent { get; set; }
    public long BytesReceived { get; set; }
    public string Category { get; set; } = "Unknown";
}
