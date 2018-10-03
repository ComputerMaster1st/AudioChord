using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace AudioChord.Processors
{
    /// <summary>
    /// Convert youtube url links to opus audio data
    /// </summary>
    internal class YouTubeProcessor
    {
        private YoutubeClient Client = new YoutubeClient();
        private FFMpegEncoder encoder = new FFMpegEncoder();

        public static string ProcessorPrefix { get; } = "YOUTUBE";

        private async Task<SongMetadata> GetVideoMetadataAsync(string youtubeVideoId)
        {
            Video videoInfo = await Client.GetVideoAsync(youtubeVideoId);

            if (videoInfo.Duration.TotalMinutes > 15.0)
                throw new ArgumentOutOfRangeException("Video duration longer than 15 minutes!");

            return new SongMetadata(videoInfo.Title, videoInfo.Duration, videoInfo.Author, videoInfo.GetShortUrl());
        }

        /// <summary>
        /// Convert the youtube video to a <see cref="Song"/>
        /// </summary>
        /// <exception cref="ArgumentException">The videoId passed to this method is not a valid youtube video id</exception>
        /// <returns>A new <see cref="Song"/> with metadata of the Youtube video</returns>
        internal async Task<ISong> ExtractSongAsync(string videoId)
        {
            if (!YoutubeClient.ValidateVideoId(videoId))
                throw new ArgumentException("The videoId is not correctly formatted");

            //retrieve the metadata of the video
            SongMetadata metadata = await GetVideoMetadataAsync(videoId);

            //retrieve the actual vdeo and convert it to opus
            MuxedStreamInfo StreamInfo = (await Client.GetVideoMediaStreamInfosAsync(videoId)).Muxed.WithHighestVideoQuality();
            using (MediaStream youtubeStream = await Client.GetMediaStreamAsync(StreamInfo))
            {
                //convert it to a Song class
                //the processor should be responsible for prefixing the id with the correct type
                return new Song(new SongId(ProcessorPrefix, videoId), metadata, await encoder.ProcessAsync(youtubeStream));
            }
        }

        /// <summary>
        /// Retrieve all video's out of a youtube playlist
        /// </summary>
        /// <param name="playlistLocation">The url to the targeted playlist</param>
        /// <returns></returns>
        internal async Task<List<string>> ParsePlaylistAsync(Uri playlistLocation)
        {
            if (playlistLocation is null)
                throw new ArgumentNullException("The uri passed to this method is null");

            if (!YoutubeClient.TryParsePlaylistId(playlistLocation.ToString(), out string playlistId))
                throw new ArgumentException("Invalid playlist url given");

            YoutubeExplode.Models.Playlist playlist = await Client.GetPlaylistAsync(playlistId);

            //retrieve all the id's and return it as a list
            return playlist.Videos
                .ToList()
                //create tasks out of all the videos
                .ConvertAll(new Converter<Video, string>((video) => { return video.Id; }));
        }

    }
}
