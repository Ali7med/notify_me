using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using NotifyMe.Models;

namespace NotifyMe.Core.Services;

public class DataLogger : IDisposable
{
    private readonly string _connectionString;
    private readonly SqliteConnection _connection;

    public DataLogger()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appData, "NotifyMe");
        Directory.CreateDirectory(appFolder);
        
        var dbPath = Path.Combine(appFolder, "network_logs.db");
        _connectionString = $"Data Source={dbPath}";
        
        _connection = new SqliteConnection(_connectionString);
        _connection.Open();
        
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        var createTableCmd = _connection.CreateCommand();
        createTableCmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS NetworkLogs (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Timestamp TEXT NOT NULL,
                IsConnected INTEGER NOT NULL,
                DownloadSpeedBps REAL NOT NULL,
                UploadSpeedBps REAL NOT NULL,
                Latency INTEGER NOT NULL
            )";
        createTableCmd.ExecuteNonQuery();
    }

    public void LogNetworkStats(bool isConnected, double downloadSpeedBps, double uploadSpeedBps, long latency)
    {
        try
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO NetworkLogs (Timestamp, IsConnected, DownloadSpeedBps, UploadSpeedBps, Latency)
                VALUES (@timestamp, @isConnected, @downloadSpeedBps, @uploadSpeedBps, @latency)";
            
            cmd.Parameters.AddWithValue("@timestamp", DateTime.Now.ToString("o"));
            cmd.Parameters.AddWithValue("@isConnected", isConnected ? 1 : 0);
            cmd.Parameters.AddWithValue("@downloadSpeedBps", downloadSpeedBps);
            cmd.Parameters.AddWithValue("@uploadSpeedBps", uploadSpeedBps);
            cmd.Parameters.AddWithValue("@latency", latency);
            
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error logging network stats: {ex.Message}");
        }
    }

    public List<NetworkLog> GetLogs(DateTime from, DateTime to)
    {
        var logs = new List<NetworkLog>();
        
        try
        {
            var cmd = _connection.CreateCommand();
            cmd.CommandText = @"
                SELECT Id, Timestamp, IsConnected, DownloadSpeedBps, UploadSpeedBps, Latency
                FROM NetworkLogs
                WHERE Timestamp >= @from AND Timestamp <= @to
                ORDER BY Timestamp DESC";
            
            cmd.Parameters.AddWithValue("@from", from.ToString("o"));
            cmd.Parameters.AddWithValue("@to", to.ToString("o"));
            
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                logs.Add(new NetworkLog
                {
                    Id = reader.GetInt32(0),
                    Timestamp = DateTime.Parse(reader.GetString(1)),
                    IsConnected = reader.GetInt32(2) == 1,
                    DownloadSpeedBps = reader.GetDouble(3),
                    UploadSpeedBps = reader.GetDouble(4),
                    Latency = reader.GetInt64(5)
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error retrieving logs: {ex.Message}");
        }
        
        return logs;
    }

    public void ClearOldLogs(int daysToKeep = 30)
    {
        try
        {
            var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "DELETE FROM NetworkLogs WHERE Timestamp < @cutoffDate";
            cmd.Parameters.AddWithValue("@cutoffDate", cutoffDate.ToString("o"));
            
            var deleted = cmd.ExecuteNonQuery();
            System.Diagnostics.Debug.WriteLine($"Deleted {deleted} old log entries");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error clearing old logs: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _connection?.Dispose();
    }
}
