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

        public string VideoId { get; private set; }
        public SongMeta Metadata { get; private set; }

        internal static async Task<YouTubeProcessor> RetrieveAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException("The url given is either null or empty!");

            YouTubeProcessor processor = new YouTubeProcessor();

            if (!YoutubeClient.TryParseVideoId(url, out string videoId))
                throw new ArgumentException("Video Url could not be parsed!");

            await processor.GetVideoMetadataAsync(videoId);

            return processor;
        }

        private async Task GetVideoMetadataAsync(string youtubeVideoId)
        {
            VideoId = youtubeVideoId;
            Video videoInfo = await Client.GetVideoAsync(VideoId);

            if (videoInfo.Duration.TotalMinutes > 15.0)
                throw new ArgumentOutOfRangeException("Video duration longer than 15 minutes!");

            Metadata = new SongMeta(videoInfo.Title, videoInfo.Duration, videoInfo.Author);
        }

        internal async Task<Stream> ProcessAudioAsync()
        {
            AudioStreamInfo StreamInfo = (await Client.GetVideoMediaStreamInfosAsync(VideoId)).Audio.WithHighestBitrate();
            Stream opusStream;

            using (MediaStream youtubeAudioStream = await Client.GetMediaStreamAsync(StreamInfo))
            {
                opusStream = await encoder.ProcessAsync(youtubeAudioStream);
            }

            return opusStream;
        }
    }
}
