using Shared.Music.Collections.Models;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Shared.Music.Processors
{
    /// <summary>
    /// Convert discord url links to opus audio data
    /// </summary>
    internal class DiscordProcessor
    {
        private WebClient client = new WebClient();
        private FFMpegEncoder encoder = new FFMpegEncoder();

        public string AttachmentId { get; private set; }
        public SongMeta Metadata { get; private set; }

        internal static async Task<DiscordProcessor> RetrieveAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException("The url given is either null or empty!");

            DiscordProcessor processor = new DiscordProcessor();

            return processor;
        }

        private async Task GetMetadataAsync()
        {
            // TODO: ffprobe -i wow_its_so_happy.ogg -hide_banner -show_format -print_format json -v quiet > test.json
            throw new NotImplementedException();
        }

        internal async Task<Stream> ProcessAudioAsync()
        {
            throw new NotImplementedException();
        }
    }
}
