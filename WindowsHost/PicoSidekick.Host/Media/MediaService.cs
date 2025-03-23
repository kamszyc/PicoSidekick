using Diacritics.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Control;
using WindowsMediaController;

namespace PicoSidekick.Host.Media
{
    public class MediaService
    {
        private readonly MediaManager _mediaManager;
        private readonly ILogger<MediaService> _logger;

        public MediaService(ILogger<MediaService> logger)
        {
            _mediaManager = new MediaManager();
            _logger = logger;
        }

        public async Task<MediaReading> Read()
        {
            if (!_mediaManager.IsStarted)
                await _mediaManager.StartAsync();

            var session = GetSession(_mediaManager);

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

            bool isPlaying = session != null && IsSessionPlaying(session);
            string artist = GetArtistName(mediaProperties);
            string title = mediaProperties?.Title;

            return new MediaReading
            {
                Artist = artist?.RemoveDiacritics(),
                Title = title?.RemoveDiacritics(),
                IsPlaying = isPlaying,
            };
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

        private static bool IsSessionPlaying(MediaManager.MediaSession mediaSession)
        {
            return mediaSession.ControlSession.GetPlaybackInfo().PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
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
