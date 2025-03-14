using Diacritics.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Media.Control;
using WindowsMediaController;

namespace PicoSidekick.Host
{
    public class SerialPortHostedService : BackgroundService
    {
        private readonly TrayIconFactory _trayIconFactory;
        private readonly ILogger<SerialPortHostedService> _logger;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public SerialPortHostedService(
            TrayIconFactory trayIconFactory,
            ILogger<SerialPortHostedService> logger)
        {
            _trayIconFactory = trayIconFactory;
            _logger = logger;
            _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Initializing...");

            var mediaManager = new MediaManager();
            await mediaManager.StartAsync();

            await _trayIconFactory.CreateTrayIcon();

            var computerInfo = new ComputerInfo();

            float totalRamInGigabytes = BytesToGigabytes(computerInfo.TotalPhysicalMemory);
            float totalRamInGigabytesRounded = (float)Math.Round(totalRamInGigabytes, 1);
            var cpuCounter = new PerformanceCounter("Processor Information", "% Processor Utility", "_Total");
            var availableRamCounter = new PerformanceCounter("Memory", "Available Bytes");

            SerialPort port = null;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var session = GetSession(mediaManager);

                    GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties = null;
                    if (session == null)
                    {
                        _logger.LogInformation("No media session found");
                    }
                    else
                    {
                        mediaProperties = await session
                            .ControlSession?
                            .TryGetMediaPropertiesAsync();
                    }

                    if (port == null || !port.IsOpen)
                    {
                        port = OpenPort();
                        if (port == null)
                        {
                            await Task.Delay(5000, stoppingToken);
                            continue;
                        }
                    }
                    string artist = GetArtistName(mediaProperties);
                    string title = mediaProperties?.Title;

                    float cpu = (float)Math.Round(cpuCounter.NextValue());
                    float usedRamInGigabytes = CalculateUsedRam(totalRamInGigabytes, availableRamCounter);

                    var updateRequest = new UpdateRequest
                    {
                        Time = DateTime.Now.ToShortTimeString(),
                        Artist = artist?.RemoveDiacritics(),
                        Title = title?.RemoveDiacritics(),
                        UsedCPUPercent = cpu,
                        UsedRAMGigabytes = usedRamInGigabytes,
                        TotalRAMGigabytes = totalRamInGigabytesRounded,
                    };
                    string request = JsonSerializer.Serialize(updateRequest, _jsonSerializerOptions);
                    port.WriteLine(request);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "Error, continuing...");
                }

                await Task.Delay(500, stoppingToken);
            }
        }

        private static float CalculateUsedRam(float totalRamInGigabytes, PerformanceCounter availableRamCounter)
        {
            float usedRamInGigabytes = totalRamInGigabytes - BytesToGigabytes(availableRamCounter.NextValue());
            return (float)Math.Round(usedRamInGigabytes, 1);
        }

        private static float BytesToGigabytes(float bytes)
        {
            return bytes / 1024f / 1024f / 1024f;
        }

        private SerialPort OpenPort()
        {
            string portName = ComDeviceFinder.GetCircuitPythonDataSerialPortName();
            if (portName == null)
            {
                _logger.LogInformation("No compatible Circuit Python serial port found");
                return null;
            }

            var port = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One)
            {
                DtrEnable = true
            };
            port.Open();

            _logger.LogInformation("Connected!");
            return port;
        }

        private static MediaManager.MediaSession GetSession(MediaManager mediaManager)
        {
            const string SpotifyPrefix = "Spotify";
            var spotifyMediaSession = mediaManager.CurrentMediaSessions.FirstOrDefault(s => s.Key.StartsWith(SpotifyPrefix)).Value;
            if (spotifyMediaSession != null && IsSessionPlaying(spotifyMediaSession))
            {
                return spotifyMediaSession;
            }
            var firstPlayingMediaSession = mediaManager.CurrentMediaSessions.FirstOrDefault(s => IsSessionPlaying(s.Value)).Value;
            if (firstPlayingMediaSession != null)
            {
                return firstPlayingMediaSession;
            }

            return mediaManager.CurrentMediaSessions.FirstOrDefault().Value;
        }

        private static bool IsSessionPlaying(MediaManager.MediaSession spotifyMediaSession)
        {
            return spotifyMediaSession.ControlSession.GetPlaybackInfo().PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
        }

        private static string GetArtistName(GlobalSystemMediaTransportControlsSessionMediaProperties mediaProperties)
        {
            if (!string.IsNullOrEmpty(mediaProperties?.Artist))
                return mediaProperties.Artist;

            // for podcasts
            return mediaProperties?.AlbumTitle;
        }
    }
}
