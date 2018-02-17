using MongoDB.Bson;
using Shared.Music.Collections.Models;
using Shared.Music.Processors;
using System;
using System.IO;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace Shared.Music
{
    /// <summary>
    /// Convert youtube url links to opus audio data
    /// </summary>
    internal class YouTubeProcessor
    {
        private YoutubeClient Client = new YoutubeClient();
        private FFMpegEncoder encoder = new FFMpegEncoder();

        public SongMetadata Metadata { get; private set; }

        private string VideoId;

        private YouTubeProcessor()
        { }

        /// <summary>
        /// Try to retrieve the video information using the given url
        /// </summary>
        /// <param name="url">The url to try to retrieve videos from</param>
        /// <returns>A <see cref="YouTubeProcessor"/> instance with a valid video and metadata</returns>
        /// <exception cref="ArgumentException">The video url given is invalid</exception>
        /// <exception cref="ArgumentNullException">the given url is null or empty</exception>
        internal static async Task<YouTubeProcessor> RetrieveAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException("the url given was null or empty");
            }

            YouTubeProcessor processor = new YouTubeProcessor();

            if(!YoutubeClient.TryParseVideoId(url, out string videoId))
            {
                throw new ArgumentException("video url is invalid!");
            }

            await processor.GetVideoMetadataAsync(videoId);

            return processor;
        }

        private async Task GetVideoMetadataAsync(string videoId)
        {
            VideoId = videoId;
            Video videoInfo = await Client.GetVideoAsync(VideoId);
            Metadata = new SongMetadata(videoInfo.Title, videoInfo.Duration, videoInfo.Author);
        }

        /// <summary>
        /// Convert the given video to an <see cref="Song"/> with an audio stream
        /// </summary>
        /// <returns>A song with an encoded audio stream and metadata</returns>
        internal async Task<Song> ProcessAudioAsync()
        {
            //select the highest quality audio
            AudioStreamInfo StreamInfo = (await Client.GetVideoMediaStreamInfosAsync(VideoId)).Audio.WithHighestBitrate();

            //encode the song using ffmpeg
            FFMpegEncoder encoder = new FFMpegEncoder();

            Stream song;
            using (MediaStream youtubeAudioStream = await Client.GetMediaStreamAsync(StreamInfo))
            {
                // encode the youtube stream to opus
                song = await encoder.ProcessAsync(youtubeAudioStream);
            } 

            //return a new song
            return new Song(Metadata, song, ObjectId.GenerateNewId());
        }
    }
}
