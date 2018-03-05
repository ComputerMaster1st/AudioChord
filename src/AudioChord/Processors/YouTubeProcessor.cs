using AudioChord.Collections.Models;
using AudioChord.Processors;
using System;
using System.IO;
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

        public string VideoId { get; private set; }
        public SongMetadata Metadata { get; private set; }

        internal static async Task<YouTubeProcessor> RetrieveAsync(string videoId)
        {
            YouTubeProcessor processor = new YouTubeProcessor();

            await processor.GetVideoMetadataAsync(videoId);

            return processor;
        }

        private async Task GetVideoMetadataAsync(string youtubeVideoId)
        {
            VideoId = youtubeVideoId;
            Video videoInfo = await Client.GetVideoAsync(VideoId);

            if (videoInfo.Duration.TotalMinutes > 15.0)
                throw new ArgumentOutOfRangeException("Video duration longer than 15 minutes!");

            Metadata = new SongMetadata(videoInfo.Title, videoInfo.Duration, videoInfo.Author, videoInfo.GetShortUrl());
        }

        internal async Task<Stream> ProcessAudioAsync()
        {
            AudioStreamInfo StreamInfo = (await Client.GetVideoMediaStreamInfosAsync(VideoId)).Audio.WithHighestBitrate();
            Stream opusStream;


            using (MediaStream youtubeAudioStream = await Client.GetMediaStreamAsync(StreamInfo))
            {
                MemoryStream output = new MemoryStream();
                await youtubeAudioStream.CopyToAsync(output);
                output.Position = 0;
                opusStream = await encoder.ProcessAsync(output);
            }

            return opusStream;
        }
    }
}
