using Diacritics.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO.Ports;
using System.Management;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Windows.Media.Control;
using WindowsFormsLifetime;
using WindowsMediaController;

namespace PicoMusicSidekick.Host
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);
            builder.UseWindowsFormsLifetime<ApplicationContext>();
            builder.Services.AddHostedService<SerialPortHostedService>();
            builder.Services.AddHostedService<TrayIconHostedService>();

            var app = builder.Build();
            app.Run();
        }
    }
}