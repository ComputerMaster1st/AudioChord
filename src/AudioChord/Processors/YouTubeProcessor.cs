using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioChord.Extractors;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace AudioChord.Processors
{
    /// <summary>
    /// Extracts audio from youtube videos
    /// </summary>
    internal class YouTubeProcessor : IAudioExtractor
    {
        private readonly YoutubeClient _client = new YoutubeClient();
        private readonly FFmpegEncoder _encoder = new FFmpegEncoder();

        public static string ProcessorPrefix { get; } = "YOUTUBE";
        
        public Task<ISong> ExtractAsync(string url, ExtractorConfiguration configuration)
        {
            string id = YoutubeClient.ParseVideoId(url);
            
            return ExtractSongAsync(id, configuration.MaxSongDuration);
        }

        /// <summary>
        /// Convert the youtube video to a <see cref="Song"/>
        /// </summary>
        /// <exception cref="ArgumentException">The videoId passed to this method is not a valid youtube video id</exception>
        /// <returns>A new <see cref="Song"/> with metadata of the Youtube video</returns>
        [Obsolete("Use the IAudioExtractor processing instead")]
        internal Task<ISong> ExtractSongAsync(string videoId)
        {
            return ExtractSongAsync(videoId, TimeSpan.FromMinutes(15));
        }

        private async Task<ISong> ExtractSongAsync(string videoId, TimeSpan maximumDuration)
        {
            if (!YoutubeClient.ValidateVideoId(videoId))
                throw new ArgumentException("The videoId is not correctly formatted");

            // Retrieve the metadata of the video
            SongMetadata metadata = await GetVideoMetadataAsync(videoId);

            if (metadata.Length > maximumDuration)
                throw new ArgumentOutOfRangeException(nameof(videoId), "Video duration longer than 15 minutes!");

            // Retrieve the actual video and convert it to opus
            MuxedStreamInfo streamInfo =
                (await _client.GetVideoMediaStreamInfosAsync(videoId)).Muxed.WithHighestVideoQuality();
            
            using (MediaStream youtubeStream = await _client.GetMediaStreamAsync(streamInfo))
            {
                // Convert it to a Song class
                // The processor should be responsible for prefixing the id with the correct type
                return new Song(new SongId(ProcessorPrefix, videoId), metadata,
                    await _encoder.ProcessAsync(youtubeStream));
            }
        }
        
        private async Task<SongMetadata> GetVideoMetadataAsync(string youtubeVideoId)
        {
            Video videoInfo = await _client.GetVideoAsync(youtubeVideoId);

            return new SongMetadata(videoInfo.Title, videoInfo.Duration, videoInfo.Author, videoInfo.GetShortUrl());
        }

        /// <summary>
        /// Retrieve all video's out of a youtube playlist
        /// </summary>
        /// <param name="playlistLocation">The url to the targeted playlist</param>
        /// <returns></returns>
        internal async Task<List<string>> ParsePlaylistAsync(Uri playlistLocation)
        {
            if (playlistLocation is null)
                throw new ArgumentNullException(nameof(playlistLocation), "The uri passed to this method is null");

            if (!YoutubeClient.TryParsePlaylistId(playlistLocation.ToString(), out string playlistId))
                throw new ArgumentException("Invalid playlist url given", nameof(playlistLocation));

            YoutubeExplode.Models.Playlist playlist = await _client.GetPlaylistAsync(playlistId);

            //retrieve all the id's and return it as a list
            return playlist.Videos
                .ToList()
                //create tasks out of all the videos
                .ConvertAll(video => video.GetUrl());
        }
    }
}