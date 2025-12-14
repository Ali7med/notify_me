using System;
using System.IO;
using System.Windows.Media;

namespace NotifyMe.Core.Services;

public class SoundService
{
    private readonly MediaPlayer _mediaPlayer;
    private bool _isEnabled;
    private readonly string _soundsPath;

    public SoundService()
    {
        _mediaPlayer = new MediaPlayer();
        
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
        PlaySound("disconnect.wav");
    }

    public void PlayConnectionRestored()
    {
        PlaySound("reconnect.wav");
    }

    private void PlaySound(string fileName)
    {
        if (!_isEnabled) return;

        try
        {
            var soundFile = Path.Combine(_soundsPath, fileName);
            if (File.Exists(soundFile))
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        _mediaPlayer.Open(new Uri(soundFile));
                        _mediaPlayer.Play();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error playing sound {fileName}: {ex.Message}");
                        // Fallback
                        System.Media.SystemSounds.Exclamation.Play();
                    }
                });
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Sound file missing: {soundFile}");
                System.Media.SystemSounds.Hand.Play();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"General sound error: {ex.Message}");
        }
    }
}
