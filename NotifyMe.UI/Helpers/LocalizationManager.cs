using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Windows;
using NotifyMe.UI.Models;

namespace NotifyMe.UI.Helpers;

public static class LocalizationManager
{
    private static LanguageData? _currentLanguage;
    private static readonly Dictionary<string, LanguageData> _availableLanguages = new();
    private static readonly string _languagesFolder;

    static LocalizationManager()
    {
        // Languages folder in app directory
        var appFolder = AppDomain.CurrentDomain.BaseDirectory;
        _languagesFolder = Path.Combine(appFolder, "Languages");
        
        // Also check AppData for user-added languages
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var userLanguagesFolder = Path.Combine(appData, "NotifyMe", "Languages");
        
        // Create folders if they don't exist
        Directory.CreateDirectory(_languagesFolder);
        Directory.CreateDirectory(userLanguagesFolder);
        
        LoadAllLanguages();
        
        // Default to English if no languages loaded
        if (!_availableLanguages.Any())
        {
            _currentLanguage = CreateFallbackEnglish();
        }
        else
        {
            _currentLanguage = _availableLanguages.Values.FirstOrDefault() ?? CreateFallbackEnglish();
        }
    }

    private static void LoadAllLanguages()
    {
        _availableLanguages.Clear();
        
        // Load from app Languages folder
        LoadLanguagesFromFolder(_languagesFolder);
        
        // Load from AppData (user-added languages)
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var userLanguagesFolder = Path.Combine(appData, "NotifyMe", "Languages");
        LoadLanguagesFromFolder(userLanguagesFolder);
    }

    private static void LoadLanguagesFromFolder(string folder)
    {
        if (!Directory.Exists(folder))
            return;

        foreach (var jsonFile in Directory.GetFiles(folder, "*.json"))
        {
            try
            {
                var json = File.ReadAllText(jsonFile);
                var language = JsonSerializer.Deserialize<LanguageData>(json);
                
                if (language != null && !string.IsNullOrEmpty(language.LanguageCode))
                {
                    _availableLanguages[language.LanguageCode] = language;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading language file {jsonFile}: {ex.Message}");
            }
        }
    }

    public static List<LanguageData> GetAvailableLanguages()
    {
        return _availableLanguages.Values.OrderBy(l => l.LanguageName).ToList();
    }

    public static LanguageData Current => _currentLanguage ?? CreateFallbackEnglish();
    public static LanguageData CurrentLanguage => Current; // Alias for compatibility

    public static event EventHandler<EventArgs>? LanguageChanged;

    public static void LoadLanguage(string languageCode)
    {
        if (_availableLanguages.TryGetValue(languageCode, out var language))
        {
            _currentLanguage = language;
            ChangeCulture(languageCode);
            LanguageChanged?.Invoke(null, EventArgs.Empty);
        }
    }

    public static void ChangeCulture(string culture)
    {
        var cultureInfo = new CultureInfo(culture);
        
        // Set culture for current thread
        Thread.CurrentThread.CurrentCulture = cultureInfo;
        Thread.CurrentThread.CurrentUICulture = cultureInfo;
        
        // Set culture for WPF
        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
        
        // Set FlowDirection based on language RTL property
        var flowDirection = _currentLanguage?.IsRTL == true 
            ? FlowDirection.RightToLeft 
            : FlowDirection.LeftToRight;
        
        foreach (Window window in Application.Current.Windows)
        {
            // Skip FloatingIconWindow - it should remain LTR (numbers only)
            if (window.GetType().Name == "FloatingIconWindow")
                continue;
                
            window.FlowDirection = flowDirection;
        }
    }

    private static LanguageData CreateFallbackEnglish()
    {
        return new LanguageData
        {
            LanguageName = "English",
            LanguageCode = "en",
            IsRTL = false,
            Settings = new SettingsStrings
            {
                Title = "⚙️ Settings",
                SaveChanges = "💾 Save Changes",
                // ... add minimal fallback strings
            },
            ContextMenu = new ContextMenuStrings
            {
                ShowStatistics = "Show Statistics",
                Settings = "Settings",
                Exit = "Exit"
            },
            MainWindow = new MainWindowStrings
            {
                Title = "NotifyMe",
                Connected = "Connected",
                Disconnected = "Disconnected"
            },
            Notifications = new NotificationStrings
            {
                ConnectionLost = "Connection Lost",
                ConnectionRestored = "Connection Restored"
            }
        };
    }
}
