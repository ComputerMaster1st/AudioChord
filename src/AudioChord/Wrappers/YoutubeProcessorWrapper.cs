using AudioChord.Collections;
using System;
using System.Threading.Tasks;
using YoutubeExplode;

namespace AudioChord.Wrappers
{
    public class YoutubeProcessorWrapper
    {
        private SongCollection songCollection;
        private PlaylistProcessor playlistProcessor;

        internal YoutubeProcessorWrapper(SongCollection songStorage, PlaylistProcessor processor)
        {
            songCollection = songStorage;
            playlistProcessor = processor;
        }

        /// <summary>
        /// Download a list of YT songs to database.
        /// </summary>
        public Task<ResolvingPlaylist> DownloadPlaylistAsync(Uri playlistLocation) => playlistProcessor.PreProcessPlaylist(playlistLocation);

        /// <summary>
        /// Download song from YouTube to database. (Note: Exceptions are to be expected.)
        /// </summary>
        /// <param name="url">The youtube video url.</param>
        /// <returns>A new <see cref="ISong"/> with the audio of the youtube video</returns>
        /// <exception cref="FormatException">The url given was not a valid youtube url</exception>
        public Task<ISong> DownloadAsync(Uri url)
        {
            if(YoutubeClient.TryParseVideoId(url.ToString(), out string id))
                return songCollection.DownloadFromYouTubeAsync(id);

            throw new FormatException("Invalid youtube video URL");
        }

        /// <summary>
        /// Capture Youtube Video Id
        /// </summary>
        /// <param name="url">The youtube video url.</param>
        /// <param name="videoId">The <paramref name="videoId"/> of the youtube url</param>
        /// <returns><see langword="true"/>if the capturing was succesfull</returns>
        public bool TryParseYoutubeUrl(string url, out string videoId)
        {
            if(YoutubeClient.TryParseVideoId(url, out string id))
            {
                videoId = id;
                return true;
            }

            videoId = null;
            return false;
        }
    }
}