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
    internal class YouTubeProcessor
    {
        private YoutubeClient Client = new YoutubeClient();
        private FFMpegEncoder encoder = new FFMpegEncoder();
        private Song result;

        public SongMetadata Metadata { get; private set; }

        private string VideoId;

        private YouTubeProcessor()
        { }

        internal static async Task<YouTubeProcessor> RetrieveAsync(string url)
        {
            YouTubeProcessor processor = new YouTubeProcessor();

            if(!YoutubeClient.TryParseVideoId(url, out string videoId))
            {
                throw new ArgumentException("video url is invalid!");
            }

            await processor.GetVideoMetadataAsync();

            return processor;
        }

        //obtain metadata
        //validate if too long
        //obtain audio stream metadata
        //(?)select audio stream
        //retrieve selected audio stream
        //convert audio to opus
        //create and return song
        //return song

        private async Task GetVideoMetadataAsync()
        {
            Video videoInfo = await Client.GetVideoAsync(VideoId);
            Metadata = new SongMetadata(videoInfo.Title, videoInfo.Duration, videoInfo.Author);
        }

        internal async Task<bool> ProcessAudioAsync()
        {
            try
            {
                AudioStreamInfo StreamInfo = (await Client.GetVideoMediaStreamInfosAsync(VideoId)).Audio.WithHighestBitrate();

                string Filename = $"{VideoId}.{StreamInfo.Container.GetFileExtension()}";
                await Client.DownloadMediaStreamAsync(StreamInfo, Filename);

                return true;
            } catch
            {
                return false;
            }
        }
    }
}
