using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace AudioChord.Processors
{
    /// <summary>
    /// Extracts audio metadata from any given url
    /// </summary>
    public class FFprobeMetadataExtractor
    {
        /// <summary>
        /// Returns all possible <see cref="SongMetadata"/> found in the stream />
        /// </summary>
        /// <param name="url">The url to probe for audio metadata</param>
        /// <returns>An <see cref="SongMetadata"/>Filled with all known values</returns>
        public Task<SongMetadata> GetMetadataAsync(string url)
            => GetMetadataAsync(url, new SongMetadata());

        public async Task<SongMetadata> GetMetadataAsync(string url, SongMetadata existingMetadata)
        {
            JObject root = await ProbeFileAsync(url);

            if (existingMetadata.Source != url)
                existingMetadata.Source = url;
            
            if (!root["format"].HasValues)
                throw new ArgumentException("FFprobe gave back unreadable information.");

            existingMetadata.Duration = TimeSpan.FromSeconds(root["format"]["duration"].Value<double>());

            if (root["format"]["tags"] != null && root["format"]["tags"].HasValues &&
                ((JObject) root["format"]["tags"]).TryGetValue("title", out JToken title)) 
                existingMetadata.Title = title.ToString();

            return existingMetadata;
        }
        
        private static async Task<JObject> ProbeFileAsync(string url)
        {
            TaskCompletionSource<int> awaitExitSource = new TaskCompletionSource<int>();
            string json;

            using (Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffprobe",
                    Arguments = $"-i {url} -hide_banner -show_format -print_format json -v quiet",
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
            
            if(await awaitExitSource.Task != 0) 
                throw new Exception("FFprobe closed with a non-0 exit code");

            return JObject.Parse(json);
        }
    }
}