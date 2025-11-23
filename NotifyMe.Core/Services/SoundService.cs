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
        
        // Get path to Resources/Sounds folder
        var appPath = AppDomain.CurrentDomain.BaseDirectory;
        _soundsPath = Path.Combine(appPath, "Resources", "Sounds");
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set => _isEnabled = value;
    }

    public void PlayConnectionLost()
    {
        if (!_isEnabled) return;

        try
        {
            var soundFile = Path.Combine(_soundsPath, "disconnect.wav");
            if (File.Exists(soundFile))
            {
                _soundPlayer.SoundLocation = soundFile;
                _soundPlayer.Play();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to play disconnect sound: {ex.Message}");
        }
    }

    public void PlayConnectionRestored()
    {
        if (!_isEnabled) return;

        try
        {
            var soundFile = Path.Combine(_soundsPath, "reconnect.wav");
            if (File.Exists(soundFile))
            {
                _soundPlayer.SoundLocation = soundFile;
                _soundPlayer.Play();
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
