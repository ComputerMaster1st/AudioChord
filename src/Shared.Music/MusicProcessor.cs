using System;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace Shared.Music
{
    internal class MusicProcessor
    {
        private YoutubeClient Client = new YoutubeClient();
        private string VideoId;
        private string Name;
        private TimeSpan Length;
        private string Author;
        private AudioStreamInfo StreamInfo;

        internal MusicProcessor(string Url)
        {
            VideoId = YoutubeClient.ParseVideoId(Url);
        }

        internal async Task<bool> ObtainVideoAsync()
        {
            try
            {
                Video VInfo = await Client.GetVideoAsync(VideoId);

                Name = VInfo.Title;
                Length = VInfo.Duration;
                Author = VInfo.Author;

                return true;
            } catch
            {
                return false;
            }
        }

        internal async Task<bool> ObtainAudioStreamAsync()
        {
            try
            {
                MediaStreamInfoSet StreamInfoSet = await Client.GetVideoMediaStreamInfosAsync(VideoId);
                StreamInfo = StreamInfoSet.Audio.WithHighestBitrate();

                return true;
            } catch
            {
                return false;
            }
        }
    }
}
