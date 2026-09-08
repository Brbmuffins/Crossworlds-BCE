using System.Diagnostics;
using System.IO;
using System.Windows;
using Velopack;

namespace Crossworlds.Launcher;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        VelopackApp.Build().Run();

        if (args.Any(arg => string.Equals(arg, "--smoke-test", StringComparison.OrdinalIgnoreCase)))
        {
            string gamePath = Path.Combine(AppContext.BaseDirectory, "Game", "Crossworlds.exe");
            Environment.ExitCode = File.Exists(gamePath) ? 0 : 2;
            return;
        }

        var application = new Application
        {
            ShutdownMode = ShutdownMode.OnMainWindowClose
        };
        application.Run(new LauncherWindow(args));
    }
}

internal sealed class LauncherWindow : Window
{
    private const string UpdateFeed = "https://playcrossworlds.com/downloads/updates/win-x64";
    private readonly string[] startupArgs;
    private readonly System.Windows.Controls.TextBlock status;
    private readonly System.Windows.Controls.Button playButton;
    private readonly System.Windows.Controls.ProgressBar progress;
    private bool ready;

    public LauncherWindow(string[] args)
    {
        startupArgs = args;
        Title = "Crossworlds";
        Width = 760;
        Height = 430;
        MinWidth = 640;
        MinHeight = 360;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = new System.Windows.Media.SolidColorBrush(
            System.Windows.Media.Color.FromRgb(12, 9, 17));

        var panel = new System.Windows.Controls.Grid { Margin = new Thickness(46) };
        panel.RowDefinitions.Add(new System.Windows.Controls.RowDefinition());
        panel.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = GridLength.Auto });

        var heading = new System.Windows.Controls.StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center
        };
        heading.Children.Add(new System.Windows.Controls.TextBlock
        {
            Text = "CROSSWORLDS",
            FontFamily = new System.Windows.Media.FontFamily("Georgia"),
            FontSize = 44,
            FontWeight = FontWeights.Bold,
            Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(224, 184, 82)),
            HorizontalAlignment = HorizontalAlignment.Center
        });
        heading.Children.Add(new System.Windows.Controls.TextBlock
        {
            Text = "BEYOND THE CELESTIAL EDGE",
            FontFamily = new System.Windows.Media.FontFamily("Georgia"),
            FontSize = 14,
            Margin = new Thickness(0, 7, 0, 28),
            Foreground = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(164, 126, 192)),
            HorizontalAlignment = HorizontalAlignment.Center
        });

        status = new System.Windows.Controls.TextBlock
        {
            Text = "Preparing launcher…",
            FontSize = 14,
            Foreground = System.Windows.Media.Brushes.Gainsboro,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        heading.Children.Add(status);

        progress = new System.Windows.Controls.ProgressBar
        {
            Height = 8,
            Width = 520,
            Margin = new Thickness(0, 14, 0, 0),
            IsIndeterminate = true
        };
        heading.Children.Add(progress);
        panel.Children.Add(heading);

        playButton = new System.Windows.Controls.Button
        {
            Content = "PLAY",
            Width = 190,
            Height = 48,
            Margin = new Thickness(0, 28, 0, 0),
            FontFamily = new System.Windows.Media.FontFamily("Georgia"),
            FontSize = 19,
            FontWeight = FontWeights.Bold,
            IsEnabled = false,
            Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(121, 79, 20)),
            Foreground = System.Windows.Media.Brushes.White
        };
        playButton.Click += (_, _) => LaunchGame();
        System.Windows.Controls.Grid.SetRow(playButton, 1);
        panel.Children.Add(playButton);
        Content = panel;

        Loaded += async (_, _) => await PrepareAsync();
    }

    private async Task PrepareAsync()
    {
        if (IsSteamLaunch())
        {
            SetReady("Steam manages updates for this installation.");
            return;
        }

        try
        {
            var updates = new UpdateManager(UpdateFeed);
            if (!updates.IsInstalled)
            {
                SetReady("Development build — automatic updates are disabled.");
                return;
            }

            status.Text = "Checking for updates…";
            var update = await updates.CheckForUpdatesAsync();
            if (update == null)
            {
                SetReady("Game is up to date.");
                return;
            }

            status.Text = $"Downloading version {update.TargetFullRelease.Version}…";
            progress.IsIndeterminate = true;
            await updates.DownloadUpdatesAsync(update);
            status.Text = "Installing update…";
            updates.ApplyUpdatesAndRestart(update, new[] { "--updated" });
        }
        catch (Exception ex)
        {
            SetReady("Update server unavailable. You may play the installed version.");
            Debug.WriteLine(ex);
        }
    }

    private void SetReady(string message)
    {
        ready = true;
        status.Text = message;
        progress.IsIndeterminate = false;
        progress.Value = 100;
        playButton.IsEnabled = true;
    }

    private bool IsSteamLaunch() =>
        startupArgs.Any(arg => string.Equals(arg, "--distribution=steam", StringComparison.OrdinalIgnoreCase)) ||
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SteamAppId"));

    private void LaunchGame()
    {
        if (!ready) return;
        string gamePath = Path.Combine(AppContext.BaseDirectory, "Game", "Crossworlds.exe");
        if (!File.Exists(gamePath))
        {
            MessageBox.Show(this, "The game executable is missing. Please reinstall Crossworlds.",
                "Crossworlds", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        string distribution = IsSteamLaunch() ? "steam" : "direct";
        Process.Start(new ProcessStartInfo
        {
            FileName = gamePath,
            Arguments = $"--distribution={distribution}",
            WorkingDirectory = Path.GetDirectoryName(gamePath)!,
            UseShellExecute = true
        });
        Close();
    }
}
