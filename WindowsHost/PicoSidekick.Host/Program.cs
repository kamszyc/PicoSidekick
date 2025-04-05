using Diacritics.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PicoSidekick.Host.Media;
using PicoSidekick.Host.Performance;
using PicoSidekick.Host.Settings;
using System.Diagnostics;
using System.IO.Ports;
using System.Management;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Windows.Media.Control;
using WindowsFormsLifetime;
using WindowsMediaController;

namespace PicoSidekick.Host
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if (IsAnotherInstanceRunning())
            {
                MessageBox.Show("App is already running!", "Pico Sidekick", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);
            builder.Services.AddWindowsFormsLifetime<SidekickApplicationContext>(
                configure: options => options.HighDpiMode = HighDpiMode.PerMonitorV2,
                preApplicationRunAction: serviceProvider => Application.SetColorMode(SystemColorMode.System));
            builder.Services.AddHostedService<SerialPortHostedService>();
            builder.Services.AddSingleton<PerformanceService>();
            builder.Services.AddSingleton<MediaService>();
            builder.Services.AddSingleton<SettingsService>();
            builder.Services.AddScoped<SettingsForm>();

            var app = builder.Build();
            app.Run();
        }

        static bool IsAnotherInstanceRunning()
        {
            using Process current = Process.GetCurrentProcess();
            string currentPath = current.MainModule?.FileName ?? "";

            return Process.GetProcessesByName(current.ProcessName)
                .Any(p =>
                {
                    if (p.Id == current.Id)
                        return false;

                    try
                    {
                        return (p.MainModule?.FileName ?? "") == currentPath;
                    }
                    catch
                    {
                        // Access denied to other process info (e.g., from another user session)
                        return false;
                    }
                });
        }
    }
}