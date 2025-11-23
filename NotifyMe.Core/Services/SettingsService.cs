using System;
using System.IO;
using System.Text.Json;
using NotifyMe.Models;

namespace NotifyMe.Core.Services;

public class SettingsService
{
    private readonly string _filePath;
    private UserSettings _currentSettings;

    public event EventHandler<UserSettings>? SettingsChanged;

    public UserSettings CurrentSettings => _currentSettings;

    public SettingsService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appData, "NotifyMe");
        Directory.CreateDirectory(appFolder);
        _filePath = Path.Combine(appFolder, "appsettings.json");
        
        _currentSettings = LoadSettings();
    }

    private UserSettings LoadSettings()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<UserSettings>(json) ?? new UserSettings();
            }
        }
        catch
        {
            // Ignore errors and use defaults
        }
        return new UserSettings();
    }

    public void SaveSettings(UserSettings settings)
    {
        try
        {
            _currentSettings = settings;
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
            SettingsChanged?.Invoke(this, _currentSettings);
        }
        catch
        {
            // Handle save error
        }
    }
}
