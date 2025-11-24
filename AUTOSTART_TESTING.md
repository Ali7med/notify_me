# Auto-Start Feature - Testing Guide

## ✅ Implementation Complete

Auto-start with Windows feature has been successfully implemented!

---

## 🎯 What Was Implemented

### 1. **UserSettings Model** ✅
- Added `StartWithWindows` property (default: `false`)
- File: `NotifyMe.Models/UserSettings.cs`

### 2. **AutoStartService** ✅
- Registry management for Windows startup
- Methods: `IsEnabled()`, `Enable()`, `Disable()`
- Uses `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run`
- File: `NotifyMe.Core/Services/AutoStartService.cs`

### 3. **Settings Window UI** ✅
- Added "🚀 Start with Windows" checkbox in Advanced tab
- Clean design integrated with existing settings
- File: `NotifyMe.UI/SettingsWindow.xaml`

### 4. **Settings Logic** ✅
- Initialize checkbox from settings
- Apply registry changes on save
- Error handling with user feedback
- File: `NotifyMe.UI/SettingsWindow.xaml.cs`

### 5. **App Integration** ✅
- AutoStartService initialization
- Automatic sync with registry state on startup
- Pass service to SettingsWindow
- File: `NotifyMe.UI/App.xaml.cs`

---

## 🧪 Testing Instructions

### Test 1: Enable Auto-Start
**Steps:**
1. Run the application: `dotnet run --project NotifyMe.UI/NotifyMe.UI.csproj`
2. Right-click system tray icon → Settings
3. Go to "⚙️ Advanced" tab
4. Check "🚀 Start with Windows"
5. Click "💾 Save Changes"

**Verification:**
```powershell
# Check Registry (PowerShell)
Get-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "NotifyMe"
```

**Expected:**
- Registry key should exist with path to NotifyMe.UI.exe
- No error messages

---

### Test 2: Verify Auto-Start Works
**Steps:**
1. Close the application completely
2. Restart Windows (or log out and log in)

**Expected:**
- NotifyMe should start automatically
- Floating widget appears
- System tray icon is visible

---

### Test 3: Disable Auto-Start
**Steps:**
1. Open Settings → Advanced tab
2. Uncheck "🚀 Start with Windows"
3. Click "💾 Save Changes"

**Verification:**
```powershell
# Check Registry (PowerShell)
Get-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "NotifyMe" -ErrorAction SilentlyContinue
```

**Expected:**
- Registry key should be removed
- Command returns nothing or "NotifyMe" property not found

---

### Test 4: Settings Sync
**Steps:**
1. Enable auto-start via Settings
2. Manually delete registry key:
   ```powershell
   Remove-ItemProperty -Path "HKCU:\Software\Microsoft\Windows\CurrentVersion\Run" -Name "NotifyMe"
   ```
3. Restart the application

**Expected:**
- Settings window shows checkbox as unchecked (synced with registry)
- Settings file updated to match actual state

---

### Test 5: Error Handling
**Steps:**
1. (Unlikely but possible) Try enabling with restricted permissions
2. Check for error message

**Expected:**
- User-friendly error message: "Failed to enable auto-start. Please check your permissions."
- Settings revert to disabled state

---

## 📝 Manual Registry Check

**Registry Location:**
```
HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run
```

**Key Name:** `NotifyMe`

**Value:** `"C:\Path\To\NotifyMe.UI.exe"`

**Via Registry Editor:**
1. Press `Win + R`
2. Type `regedit` and press Enter
3. Navigate to: `HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run`
4. Look for `NotifyMe` entry

---

## 🔧 Troubleshooting

### Issue: Auto-start doesn't work after enabling
**Solutions:**
1. Check if registry key was created
2. Verify path in registry is correct
3. Check Windows Task Manager → Startup tab
4. Ensure no antivirus blocking the registry modification

### Issue: Checkbox state doesn't match reality
**Solution:**
- This should auto-sync on app startup
- If not, manually delete settings: `%LocalAppData%\NotifyMe\appsettings.json`
- Restart app

---

## 📊 Build Status

```
Build succeeded with 141 warning(s) in 14.5s
```

✅ All files compiled successfully
✅ No errors
⚠️ Warnings are non-critical (platform-specific APIs)

---

## 📁 Files Modified

### New Files:
- `NotifyMe.Core/Services/AutoStartService.cs`

### Modified Files:
- `NotifyMe.Models/UserSettings.cs`
- `NotifyMe.UI/SettingsWindow.xaml`
- `NotifyMe.UI/SettingsWindow.xaml.cs`
- `NotifyMe.UI/App.xaml.cs`

---

## 🎓 Technical Details

### Registry Key Details:
- **Hive**: HKEY_CURRENT_USER (no admin rights needed)
- **Path**: `Software\Microsoft\Windows\CurrentVersion\Run`
- **Name**: `NotifyMe`
- **Type**: REG_SZ (String)
- **Value**: Full path to executable in quotes

### Why HKCU and not HKLM?
- **HKCU**: Current user only, no admin rights needed ✅
- **HKLM**: All users, requires admin rights ❌

### Sync Logic:
On every app startup:
1. Read settings from `appsettings.json`
2. Check actual registry state
3. If mismatch → update settings to match registry
4. This ensures settings always reflect reality

---

## ✨ Next Steps

After testing is complete:
1. Update PROGRESS.md
2. Update README.md
3. Update task.md
4. Commit changes to git

---

**Ready for testing!** 🚀
