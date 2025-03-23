using Diacritics.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PicoSidekick.Host.Media;
using PicoSidekick.Host.Performance;
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
            var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);
            builder.Services.AddWindowsFormsLifetime<SidekickApplicationContext>(preApplicationRunAction: serviceProvider => Application.SetColorMode(SystemColorMode.System));
            builder.Services.AddHostedService<SerialPortHostedService>();
            builder.Services.AddSingleton<PerformanceService>();
            builder.Services.AddSingleton<MediaService>();

            var app = builder.Build();
            app.Run();
        }
    }
}