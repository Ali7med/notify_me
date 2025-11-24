using System;
using System.IO;
using System.Media;

namespace NotifyMe.Core.Services;

public class SoundService
{
    private readonly SoundPlayer _soundPlayer;
    private bool _isEnabled;
    private readonly string _soundsPath;

    public SoundService()
    {
        _soundPlayer = new SoundPlayer();
        
        // Try to find Resources/Sounds folder
        var appPath = AppDomain.CurrentDomain.BaseDirectory;
        var possiblePaths = new[]
        {
            Path.Combine(appPath, "Resources", "Sounds"),
            Path.Combine(appPath, "..", "..", "..", "Resources", "Sounds"),
            Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Sounds"),
        };
        
        foreach (var path in possiblePaths)
        {
            if (Directory.Exists(path))
            {
                _soundsPath = Path.GetFullPath(path);
                System.Diagnostics.Debug.WriteLine($"Sound path found: {_soundsPath}");
                break;
            }
        }
        
        if (string.IsNullOrEmpty(_soundsPath))
        {
            _soundsPath = Path.Combine(appPath, "Resources", "Sounds");
            System.Diagnostics.Debug.WriteLine($"Sound path not found, using default: {_soundsPath}");
        }
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set => _isEnabled = value;
    }

    public void PlayConnectionLost()
    {
        if (!_isEnabled)
        {
            System.Diagnostics.Debug.WriteLine("Sound is disabled");
            return;
        }

        try
        {
            var soundFile = Path.Combine(_soundsPath, "disconnect.wav");
            System.Diagnostics.Debug.WriteLine($"Attempting to play: {soundFile}");
            System.Diagnostics.Debug.WriteLine($"File exists: {File.Exists(soundFile)}");
            
            if (File.Exists(soundFile))
            {
                _soundPlayer.SoundLocation = soundFile;
                _soundPlayer.Play();
                System.Diagnostics.Debug.WriteLine("Sound played successfully");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Sound file not found: {soundFile}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to play disconnect sound: {ex.Message}");
        }
    }

    public void PlayConnectionRestored()
    {
        if (!_isEnabled)
        {
            System.Diagnostics.Debug.WriteLine("Sound is disabled");
            return;
        }

        try
        {
            var soundFile = Path.Combine(_soundsPath, "reconnect.wav");
            System.Diagnostics.Debug.WriteLine($"Attempting to play: {soundFile}");
            System.Diagnostics.Debug.WriteLine($"File exists: {File.Exists(soundFile)}");
            
            if (File.Exists(soundFile))
            {
                _soundPlayer.SoundLocation = soundFile;
                _soundPlayer.Play();
                System.Diagnostics.Debug.WriteLine("Sound played successfully");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Sound file not found: {soundFile}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to play reconnect sound: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _soundPlayer?.Dispose();
    }
}
