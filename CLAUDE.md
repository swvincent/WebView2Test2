# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

All commands run from the `source/` directory.

```powershell
# Build
dotnet build

# Run
dotnet run

# Build release
dotnet build -c Release
```

There are no tests in this project.

## Architecture

Single-window WPF application targeting .NET 10 (`net10.0-windows`). The window hosts a `WebView2` control that loads `release-notes.html` from the output directory at runtime.

**Theme system** — theming operates on two layers simultaneously:
1. **WPF layer** (`ApplyWpfTheme`): sets `Background`/`Foreground` on the window and header controls programmatically, and calls `DwmSetWindowAttribute(DWMWA_USE_IMMERSIVE_DARK_MODE)` to theme the native title bar.
2. **WebView2 layer** (`ApplyWebViewTheme`): sets `CoreWebView2Profile.PreferredColorScheme`, which causes the browser engine to report the appropriate value for the CSS `prefers-color-scheme` media query that `release-notes.html` uses.

The `Auto` mode reads the registry key `HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize\AppsUseLightTheme` to determine the current OS setting.

**Initialization order matters**: `CoreWebView2` is `null` until `EnsureCoreWebView2Async()` completes in `Window_Loaded`. The `SelectionChanged` handler on the theme `ComboBox` fires during `InitializeComponent()` (before `Window_Loaded`), so `ApplyWebViewTheme` guards with a `webView?.CoreWebView2 == null` check. `ApplyTheme` is called again after `EnsureCoreWebView2Async` to apply the WebView2 side of the theme.

**HTML content**: `release-notes.html` is declared as `<Content>` in the `.csproj` with `CopyToOutputDirectory=PreserveNewest`. It is navigated to via a `file://` URI built from `AppDomain.CurrentDomain.BaseDirectory` and should not be modified — it already contains `prefers-color-scheme` CSS for both themes.
