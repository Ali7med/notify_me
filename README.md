# NotifyMe

A lightweight WPF network‑monitoring utility for Windows.

## Features
- System‑tray icon with real‑time network statistics.
- **Double‑click tray icon** opens the main window (implemented via `TrayMouseDoubleClick`).
- Configurable update intervals.
- Built for .NET 10 (net10.0‑windows10.0.26100.0).

## Prerequisites
- Windows 10+ (build 17763 or later).
- .NET SDK 10.0 installed.

## Build & Run
```bash
# From the solution root
cd d:/Apps/C#/NofiyMe
dotnet build
dotnet run --project NotifyMe.UI/NotifyMe.UI.csproj
```

## Usage
1. Launch the app – an icon appears in the system tray.
2. **Double‑click** the tray icon to restore the main window.
3. The tray tooltip shows current connection status and download/upload speeds.

## Project Structure
- `NotifyMe.Core` – core services (network monitoring, ping, statistics).
- `NotifyMe.UI` – WPF UI, tray icon, and window logic.
- `NotifyMe.Models` – data models.
- `NotifyMe.Tests` – unit tests.

## Contributing
Feel free to open issues or submit pull requests. You can extend the double‑click handler to open settings, add notification sounds, or integrate other alert channels.

## License
MIT License – see `LICENSE` file.
