using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;

namespace ReleaseNotesViewer;

public partial class MainWindow : Window
{
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    private enum ThemeMode { Auto, Light, Dark }

    private ThemeMode _currentTheme = ThemeMode.Auto;

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await webView.EnsureCoreWebView2Async();

        var htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "release-notes.html");
        webView.CoreWebView2.Navigate(new Uri(htmlPath).AbsoluteUri);

        ApplyTheme(_currentTheme);
    }

    private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (themeComboBox.SelectedItem is ComboBoxItem item)
        {
            _currentTheme = item.Tag?.ToString() switch
            {
                "Light" => ThemeMode.Light,
                "Dark"  => ThemeMode.Dark,
                _       => ThemeMode.Auto
            };
            ApplyTheme(_currentTheme);
        }
    }

    private void ApplyTheme(ThemeMode theme)
    {
        bool isDark = theme switch
        {
            ThemeMode.Dark  => true,
            ThemeMode.Light => false,
            _               => IsSystemDarkMode()
        };

        ApplyWpfTheme(isDark);
        ApplyWebViewTheme(theme);
    }

    private void ApplyWpfTheme(bool isDark)
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd != IntPtr.Zero)
        {
            int value = isDark ? 1 : 0;
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref value, sizeof(int));
        }

        if (isDark)
        {
            Background                = new SolidColorBrush(Color.FromRgb(0x1a, 0x1a, 0x1a));
            headerBorder.Background   = new SolidColorBrush(Color.FromRgb(0x2d, 0x2d, 0x2d));
            themeLabel.Foreground     = new SolidColorBrush(Color.FromRgb(0xdd, 0xdd, 0xdd));
            themeComboBox.Background  = new SolidColorBrush(Color.FromRgb(0x3d, 0x3d, 0x3d));
            themeComboBox.Foreground  = new SolidColorBrush(Color.FromRgb(0xdd, 0xdd, 0xdd));
        }
        else
        {
            Background                = SystemColors.WindowBrush;
            headerBorder.Background   = SystemColors.ControlBrush;
            themeLabel.Foreground     = SystemColors.ControlTextBrush;
            themeComboBox.Background  = SystemColors.WindowBrush;
            themeComboBox.Foreground  = SystemColors.ControlTextBrush;
        }
    }

    private void ApplyWebViewTheme(ThemeMode theme)
    {
        if (webView?.CoreWebView2 == null) return;

        webView.CoreWebView2.Profile.PreferredColorScheme = theme switch
        {
            ThemeMode.Dark  => CoreWebView2PreferredColorScheme.Dark,
            ThemeMode.Light => CoreWebView2PreferredColorScheme.Light,
            _               => CoreWebView2PreferredColorScheme.Auto
        };
    }

    private static bool IsSystemDarkMode()
    {
        using var key = Registry.CurrentUser.OpenSubKey(
            @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
        return key?.GetValue("AppsUseLightTheme") is int v && v == 0;
    }
}
