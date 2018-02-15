using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Shared.Music.Processors
{
    internal class FFMpegEncoder
    {
        ProcessStartInfo ffmpegProcessInfo;

        public FFMpegEncoder(string filepath)
        {
            ffmpegProcessInfo = new ProcessStartInfo()
            {
                FileName = "ffmpeg", //don't know how concentus will react to different bitrate (was 96k)
                Arguments = $"-i {filepath} -ar 48k -codec:a opus -b:a 128k -vbr on -vn -ac 2 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = false
            };
        }

        public Task<Stream> ProcessAsync()
        {
            TaskCompletionSource<Stream> taskCompletionSource = new TaskCompletionSource<Stream>();

            //create a new process for ffmpeg
            Process process = new Process()
            {
                StartInfo = ffmpegProcessInfo,
                EnableRaisingEvents = true
            };

            //set what to do when the process finishes working
            process.Exited += (sender, args) =>
            {
                //TODO: Add handling for when the process has an error (doesn't return 0 as exitcode)
                taskCompletionSource.SetResult(process.StandardOutput.BaseStream);
                process.Dispose();
            };

            process.Start();

            return taskCompletionSource.Task;
        }
    }
}
