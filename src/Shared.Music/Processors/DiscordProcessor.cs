using Newtonsoft.Json;
using Shared.Music.Collections.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            await processor.GetMetadataAsync(url);

            return processor;
        }

        private async Task<Dictionary<string, dynamic>> ProbeFileAsync(string filename)
        {
            TaskCompletionSource<int> awaitExitSource = new TaskCompletionSource<int>();
            string json;

            using (Process process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "ffmpeg",
                    Arguments = $"-i {filename} -hide_banner -show_format -print_format json -v quiet",
                    UseShellExecute = false,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            })
            {
                process.Exited += (obj, args) => {
                    awaitExitSource.SetResult(process.ExitCode);
                };

                process.Start();

                json = await process.StandardError.ReadToEndAsync();
                int exitCode = await awaitExitSource.Task;
            }

            return JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(json);
        }

        private async Task GetMetadataAsync(string url)
        {
            // TODO: ffprobe -i wow_its_so_happy.ogg  > test.json
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri result))
                throw new ArgumentException("Url Provided is invalid!");

            string filename = url.Substring(url.LastIndexOf('/'));

            client.DownloadFileAsync(result, filename);

            Dictionary<string, dynamic> probe = await ProbeFileAsync(filename);
        }

        internal async Task<Stream> ProcessAudioAsync()
        {
            throw new NotImplementedException();
        }
    }
}
