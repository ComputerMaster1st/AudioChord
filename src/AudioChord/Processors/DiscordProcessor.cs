using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace AudioChord.Processors
{
    /// <summary>
    /// Convert discord url links to opus audio data
    /// </summary>
    internal class DiscordProcessor
    {
        private readonly WebClient _client = new WebClient();
        private readonly FFmpegEncoder _encoder = new FFmpegEncoder();

        private string _filename;
        private ulong _attachmentId;

        private const string PROCESSOR_PREFIX = "DISCORD";

        private SongMetadata Metadata { get; set; }

        internal static async Task<DiscordProcessor> RetrieveAsync(string url, string uploader, ulong attachmentId)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url), "The url given was either null or empty!");

            DiscordProcessor processor = new DiscordProcessor {_attachmentId = attachmentId};
            await processor.GetMetadataAsync(url, uploader);
            return processor;
        }

        private async Task<string> ProbeFileAsync(string filename)
        {
            TaskCompletionSource<int> awaitExitSource = new TaskCompletionSource<int>();
            string json;

            using (Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffprobe",
                    Arguments = $"-i {filename} -hide_banner -show_format -print_format json -v quiet",
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                },
                EnableRaisingEvents = true
            })
            {
                process.Exited += (obj, args) => { awaitExitSource.SetResult(process.ExitCode); };

                process.Start();

                json = await process.StandardOutput.ReadToEndAsync();
                await awaitExitSource.Task;
            }

            return json;
        }

        private async Task GetMetadataAsync(string url, string uploader)
        {
            _filename = url.Substring(url.LastIndexOf('/') + 1);
            string songName = Path.GetFileNameWithoutExtension(_filename);
            TimeSpan length;

            await _client.DownloadFileTaskAsync(url, _filename);

            string json = await ProbeFileAsync(_filename);

            JObject root = JObject.Parse(json);
            if (!root["format"].HasValues)
                throw new ArgumentException("Unable to probe file.");

            length = TimeSpan.FromSeconds(root["format"]["duration"].Value<double>());

            if (root["format"]["tags"] != null && root["format"]["tags"].HasValues &&
                ((JObject) root["format"]["tags"]).TryGetValue("title", out JToken title)) songName = title.ToString();

            Metadata = new SongMetadata(songName, length, uploader, url);
        }

        internal async Task<Song> ProcessAudioAsync()
            => new Song(new SongId(PROCESSOR_PREFIX, _attachmentId.ToString()), Metadata,
                await _encoder.ProcessAsync(_filename));
    }
}