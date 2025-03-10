using Diacritics.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Media.Control;
using WindowsMediaController;

namespace PicoMusicSidekick.Host
{
    public class SerialPortHostedService : BackgroundService
    {
        private readonly ILogger<SerialPortHostedService> _logger;

        public SerialPortHostedService(ILogger<SerialPortHostedService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Initializing...");

            var mediaManager = new MediaManager();
            await mediaManager.StartAsync();

            SerialPort port = null;
            while (!stoppingToken.IsCancellationRequested)
            {
                var session = GetSession(mediaManager);
                if (session == null)
                {
                    _logger.LogInformation("No media session found");
                    continue;
                }

                if (port == null || !port.IsOpen)
                {
                    port = OpenPort();
                    if (port == null)
                    {
                        await Task.Delay(5000);
                        continue;
                    }
                }

                var mediaProperties = await session
                    .ControlSession?
                    .TryGetMediaPropertiesAsync();

                if (mediaProperties == null)
                    continue;

                try
                {
                    string artist = GetArtistName(mediaProperties);
                    string title = mediaProperties.Title;
                    var mediaRequest = new MediaRequest
                    {
                        Artist = artist?.RemoveDiacritics() ?? string.Empty,
                        Title = title?.RemoveDiacritics() ?? string.Empty,
                    };
                    string request = JsonSerializer.Serialize(mediaRequest, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    port.WriteLine(request);
                }
                catch (Exception)
                {
                    _logger.LogWarning("Disconnected!");
                }

                await Task.Delay(500);
            }
        }

        private SerialPort OpenPort()
        {
            string portName = ComDeviceFinder.GetCircuitPythonDataSerialPortName();
            if (portName == null)
            {
                _logger.LogInformation("No compatible Circuit Python serial port found");
                return null;
            }

            var port = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
            port.DtrEnable = true;
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
            if (!string.IsNullOrEmpty(mediaProperties.Artist))
                return mediaProperties.Artist;

            // for podcasts
            return mediaProperties.AlbumTitle;
        }
    }
}
