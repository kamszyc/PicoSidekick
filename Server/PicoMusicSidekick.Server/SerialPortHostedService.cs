using Diacritics.Extensions;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Media.Control;
using WindowsMediaController;

namespace PicoMusicSidekick.Server
{
    public class SerialPortHostedService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Initializing...");

            var mediaManager = new MediaManager();
            await mediaManager.StartAsync();

            var session = GetSession(mediaManager);
            if (session == null)
            {
                Console.Error.WriteLine("No media session found. Exiting");
                return;
            }

            SerialPort port = null;
            while (!stoppingToken.IsCancellationRequested)
            {
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
                    if (!string.IsNullOrEmpty(artist) && !string.IsNullOrEmpty(title))
                    {
                        var mediaRequest = new MediaRequest
                        {
                            Artist = artist.RemoveDiacritics(),
                            Title = title.RemoveDiacritics(),
                        };
                        string request = JsonSerializer.Serialize(mediaRequest, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                        port.WriteLine(request);
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Disconnected!");
                }

                await Task.Delay(500);
            }
        }

        private static SerialPort OpenPort()
        {
            string portName = ComDeviceFinder.GetCircuitPythonDataSerialPortName();
            if (portName == null)
            {
                Console.Error.WriteLine("No compatible Circuit Python serial port found");
                return null;
            }

            var port = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
            port.DtrEnable = true;
            port.Open();

            Console.WriteLine("Connected!");
            return port;
        }

        private static MediaManager.MediaSession GetSession(MediaManager mediaManager)
        {
            const string SpotifyPrefix = "Spotify";
            var spotifyMediaSession = mediaManager.CurrentMediaSessions.FirstOrDefault(s => s.Key.StartsWith(SpotifyPrefix)).Value;
            if (spotifyMediaSession != null)
            {
                return spotifyMediaSession;
            }
            return mediaManager.CurrentMediaSessions.FirstOrDefault().Value;
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
