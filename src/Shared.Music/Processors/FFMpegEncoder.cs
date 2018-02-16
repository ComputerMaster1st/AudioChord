using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Shared.Music.Processors
{
    public class FFMpegEncoder
    {
        private ProcessStartInfo CreateEncoderInfo(string filePath)
        {
            return new ProcessStartInfo()
            {
                FileName = @"C:\Users\Lucas\Desktop\ffmpeg-3.4.1\bin\ffmpeg.exe", //don't know how concentus will react to different bitrate (was 96k)
                Arguments = $"-i {filePath} -ar 48k -codec:a libopus -b:a 128k -ac 2 -f opus pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = false
            };
        }

        public async Task<MemoryStream> ProcessAsync(string filePath)
        {
            MemoryStream stream = new MemoryStream();
            TaskCompletionSource<int> awaitExitSource = new TaskCompletionSource<int>();

            //create a new process for ffmpeg

            using(Process process = new Process()
            {
                StartInfo = CreateEncoderInfo(filePath),
                EnableRaisingEvents = true
            })
            {
                process.Exited += (obj, args) =>
                {
                    awaitExitSource.SetResult(process.ExitCode);
                };

                process.Start();
                await process.StandardOutput.BaseStream.CopyToAsync(stream);
                stream.Position = 0;

                //wait for the exit event to happen
                int exitCode = await awaitExitSource.Task;
            }

            return stream;
        }
    }
}
