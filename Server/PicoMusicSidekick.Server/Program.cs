using System.IO.Ports;
using System.Management;
using System.Text.Json;
using System.Text.RegularExpressions;
using Windows.Media.Control;
using WindowsMediaController;

namespace PicoMusicSidekick.Server
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            var mediaManager = new MediaManager();
            await mediaManager.StartAsync();

            var session = GetSession(mediaManager);
            if (session == null)
                return;

            string portName = ComDeviceFinder.GetCircuitPythonDataSerialPortName();

            SerialPort port = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
            port.DtrEnable = true;
            port.Open();

            while (true)
            {
                var mediaProperties = await session
                    .ControlSession
                    .TryGetMediaPropertiesAsync();

                string artist = GetArtistName(mediaProperties);
                string title = mediaProperties.Title;
                if (!string.IsNullOrEmpty(artist) && !string.IsNullOrEmpty(title))
                {
                    var mediaRequest = new MediaRequest
                    {
                        Artist = artist,
                        Title = title,
                    };
                    string request = JsonSerializer.Serialize(mediaRequest, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    port.WriteLine(request);
                }

                await Task.Delay(500);
            }
        }

        private static MediaManager.MediaSession GetSession(MediaManager mediaManager)
        {
            const string SpotifyPrefix = "Spotify";
            if (mediaManager.CurrentMediaSessions.Any(s => s.Key.StartsWith(SpotifyPrefix)))
            {
                return mediaManager.CurrentMediaSessions.First(s => s.Key.StartsWith(SpotifyPrefix)).Value;
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