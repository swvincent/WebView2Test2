# Release Notes Viewer

A minimal WPF application that displays an HTML release notes file in an embedded WebView2 browser, with support for light mode, dark mode, or following the Windows system setting.

## Requirements

- Windows 10 (build 19041) or later
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Microsoft Edge WebView2 Runtime](https://developer.microsoft.com/microsoft-edge/webview2/) (included with Windows 11; install separately on Windows 10)

## Build and Run

```powershell
cd source
dotnet run
```

## Features

- Displays `release-notes.html` in an embedded WebView2 control
- Theme selector (Follow System / Light / Dark) in the toolbar
  - **Follow System** (default) reads the Windows app color mode setting
  - Themes the WebView2 content via the CSS `prefers-color-scheme` media query
  - Themes the WPF window and title bar via `DwmSetWindowAttribute`
