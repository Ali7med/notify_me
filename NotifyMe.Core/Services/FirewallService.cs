using System.Diagnostics;
using System.IO;

namespace NotifyMe.Core.Services;

/// <summary>
/// خدمة لإدارة قواعد Windows Firewall لحظر/السماح للتطبيقات
/// </summary>
public class FirewallService
{
    private const string RulePrefix = "NotifyMe_Block_";
    private readonly Dictionary<string, bool> _blockStatusCache = new();
    private DateTime _lastCacheUpdate = DateTime.MinValue;
    private const int CACHE_VALIDITY_SECONDS = 10;

    /// <summary>
    /// حظر تطبيق من الوصول للإنترنت
    /// </summary>
    public bool BlockApplication(string executablePath, string appName)
    {
        try
        {
            if (string.IsNullOrEmpty(executablePath) || !File.Exists(executablePath))
                return false;

            var ruleName = $"{RulePrefix}{appName}";

            // Check if rule already exists
            if (IsApplicationBlocked(appName))
            {
                Debug.WriteLine($"Rule already exists for {appName}");
                return true;
            }

            // Add outbound blocking rule
            var outboundResult = ExecuteNetshCommand(
                $"advfirewall firewall add rule name=\"{ruleName}_OUT\" " +
                $"dir=out action=block program=\"{executablePath}\" enable=yes"
            );

            // Add inbound blocking rule
            var inboundResult = ExecuteNetshCommand(
                $"advfirewall firewall add rule name=\"{ruleName}_IN\" " +
                $"dir=in action=block program=\"{executablePath}\" enable=yes"
            );

            var success = outboundResult && inboundResult;
            if (success)
            {
                _blockStatusCache[appName] = true; // Update cache
            }
            return success;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error blocking application: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// إلغاء حظر تطبيق
    /// </summary>
    public bool UnblockApplication(string appName)
    {
        try
        {
            var ruleName = $"{RulePrefix}{appName}";

            // Remove outbound rule
            var outboundResult = ExecuteNetshCommand(
                $"advfirewall firewall delete rule name=\"{ruleName}_OUT\""
            );

            // Remove inbound rule
            var inboundResult = ExecuteNetshCommand(
                $"advfirewall firewall delete rule name=\"{ruleName}_IN\""
            );

            var success = outboundResult && inboundResult;
            if (success)
            {
                _blockStatusCache.Remove(appName); // Update cache
            }
            return success;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error unblocking application: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// التحقق من حالة حظر التطبيق
    /// </summary>
    public bool IsApplicationBlocked(string appName)
    {
        // Refresh cache if expired
        if ((DateTime.Now - _lastCacheUpdate).TotalSeconds > CACHE_VALIDITY_SECONDS)
        {
            RefreshCache();
        }

        return _blockStatusCache.GetValueOrDefault(appName, false);
    }

    /// <summary>
    /// تحديث الـ Cache من Firewall
    /// </summary>
    private void RefreshCache()
    {
        try
        {
            _blockStatusCache.Clear();
            var blockedApps = GetBlockedApplications();
            foreach (var app in blockedApps)
            {
                _blockStatusCache[app] = true;
            }
            _lastCacheUpdate = DateTime.Now;
        }
        catch
        {
            // Keep old cache on error
        }
    }

    /// <summary>
    /// الحصول على قائمة بجميع التطبيقات المحظورة
    /// </summary>
    public List<string> GetBlockedApplications()
    {
        try
        {
            var result = ExecuteNetshCommandWithOutput(
                "advfirewall firewall show rule name=all"
            );

            var blockedApps = new List<string>();
            var lines = result.Split('\n');

            foreach (var line in lines)
            {
                if (line.Contains($"Rule Name:") && line.Contains(RulePrefix))
                {
                    var ruleName = line.Split(':')[1].Trim();
                    var appName = ruleName.Replace(RulePrefix, "").Replace("_OUT", "").Replace("_IN", "");
                    if (!blockedApps.Contains(appName))
                    {
                        blockedApps.Add(appName);
                    }
                }
            }

            return blockedApps;
        }
        catch
        {
            return new List<string>();
        }
    }

    /// <summary>
    /// تنفيذ أمر netsh
    /// </summary>
    private bool ExecuteNetshCommand(string arguments)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                Verb = "runas" // Run as administrator
            };

            using var process = Process.Start(processInfo);
            if (process == null) return false;

            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Netsh command failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// تنفيذ أمر netsh مع الحصول على النتيجة
    /// </summary>
    private string ExecuteNetshCommandWithOutput(string arguments)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(processInfo);
            if (process == null) return string.Empty;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return output;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Netsh command failed: {ex.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// حذف جميع قواعد NotifyMe من Firewall
    /// </summary>
    public bool ClearAllRules()
    {
        try
        {
            var blockedApps = GetBlockedApplications();
            foreach (var app in blockedApps)
            {
                UnblockApplication(app);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
}
