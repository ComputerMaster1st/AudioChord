using System;
using System.Threading;
using System.Threading.Tasks;
using AudioChord.Collections;
using AudioChord.Extractors;
using AudioChord.Processors;
using YoutubeExplode.Videos;

namespace AudioChord.Facades
{
    public class YoutubeProcessorFacade
    {
        private readonly SongCollection _songCollection;
        private readonly PlaylistProcessor _playlistProcessor;
        private readonly ExtractorConfiguration _defaultExtractorConfiguration;

        internal YoutubeProcessorFacade(SongCollection songStorage, PlaylistProcessor processor, ExtractorConfiguration configuration)
        {
            _songCollection = songStorage;
            _playlistProcessor = processor;
            _defaultExtractorConfiguration = configuration;
        }
        
        /// <summary>
        /// Download a list of YT songs to database (without progress).
        /// </summary>
        /// <param name="playlistLocation">The url where the playlist is located</param>
        /// <param name="configuration">
        ///     a custom extractor configuration.
        ///     if none is provided then the default settings of the <see cref="MusicServiceConfiguration"/> will be used
        /// </param>
        public Task<ResolvingPlaylist> DownloadPlaylistAsync(
                Uri playlistLocation, 
                ExtractorConfiguration configuration = null
            )
            => DownloadPlaylistAsync(playlistLocation, null, CancellationToken.None, configuration);

        /// <summary>
        /// Download a list of YT songs to database.
        /// </summary>
        /// <param name="playlistLocation">The url where the playlist is located</param>
        /// <param name="progress">Callback for reporting progress on song processing</param>
        /// <param name="token">CancellationToken to stop processing of songs in the playlist</param>
        /// <param name="configuration">
        ///     a custom extractor configuration.
        ///     if none is provided then the default settings of the <see cref="MusicServiceConfiguration"/> will be used
        /// </param>
        public Task<ResolvingPlaylist> DownloadPlaylistAsync(
            Uri playlistLocation,
            IProgress<SongProcessStatus> progress, 
            CancellationToken token,
            ExtractorConfiguration configuration = null)
        {
            ExtractorConfiguration actualConfiguration = configuration ?? _defaultExtractorConfiguration;
            
            return _playlistProcessor.ProcessPlaylist(playlistLocation, progress, token, actualConfiguration);
        }

        /// <summary>
        /// Download song from YouTube to database. (Note: Exceptions are to be expected.)
        /// </summary>
        /// <param name="url">The youtube video url.</param>
        /// <param name="configuration">
        ///     a custom extractor configuration.
        ///     if none is provided then the default settings of the <see cref="MusicServiceConfiguration"/> will be used
        /// </param>
        /// <returns>A new <see cref="ISong"/> with the audio of the youtube video</returns>
        /// <exception cref="FormatException">The url given was not a valid youtube url</exception>
        /// <exception cref="InvalidOperationException">The processed song was empty</exception>
        public Task<ISong> DownloadAsync(Uri url, ExtractorConfiguration configuration = null)
        {
            ExtractorConfiguration actualConfig = configuration ?? _defaultExtractorConfiguration;
            
            return _songCollection.DownloadFromYouTubeAsync(url.ToString(), actualConfig);
        }

        /// <summary>
        /// Capture Youtube Video Id
        /// </summary>
        /// <param name="url">The youtube video url.</param>
        /// <param name="videoId">The <paramref name="videoId"/> of the youtube url</param>
        /// <returns><see langword="true"/>if the capturing was successful</returns>
        public bool TryParseYoutubeUrl(string url, out string videoId)
        {
            VideoId? result = VideoId.TryParse(url);

            videoId = result.GetValueOrDefault();
            return result.HasValue;
        }
    }
}