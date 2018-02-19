using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Shared.Music.Processors
{
    internal class FFMpegEncoder
    {
        private ProcessStartInfo CreateEncoderInfo(string filePath, bool redirectInput = false)
        {
            // NOTE: -b:a was 96k. Change back if problems occur during playback

            return new ProcessStartInfo()
            {
                FileName = @"C:\Users\Mr_MA\Downloads\ffmpeg-20180215-fb58073-win64-static\bin\ffmpeg.exe",
                Arguments = $"-i {filePath} -ar 48k -codec:a libopus -b:a 128k -ac 2 -f opus pipe:1",
                UseShellExecute = false,
                RedirectStandardInput = redirectInput,
                RedirectStandardOutput = true
            };
        }

        internal async Task<Stream> ProcessAsync(Stream input)
        {
            MemoryStream output = new MemoryStream();
            TaskCompletionSource<int> awaitExitSource = new TaskCompletionSource<int>();

            //create a new process for ffmpeg

            using (Process process = new Process()
            {
                StartInfo = CreateEncoderInfo("pipe:0", true),
                EnableRaisingEvents = true
            })
            {
                process.Exited += (obj, args) => {
                    awaitExitSource.SetResult(process.ExitCode);
                };

                process.Start();

                // NOTE: MUST be .WhenAny & Close input to prevent lockups
                await Task.WhenAny(input.CopyToAsync(process.StandardInput.BaseStream), process.StandardOutput.BaseStream.CopyToAsync(output));
                process.StandardInput.Close();

                int exitCode = await awaitExitSource.Task;
            }

            output.Position = 0;
            return output;
        }

        internal async Task<Stream> ProcessAsync(string filePath)
        {
            MemoryStream output = new MemoryStream();
            TaskCompletionSource<int> awaitExitSource = new TaskCompletionSource<int>();

            //create a new process for ffmpeg

            using (Process process = new Process()
            {
                StartInfo = CreateEncoderInfo(filePath),
                EnableRaisingEvents = true
            })
            {
                process.Exited += (obj, args) => {
                    awaitExitSource.SetResult(process.ExitCode);
                };

                process.Start();

                await process.StandardOutput.BaseStream.CopyToAsync(output);

                int exitCode = await awaitExitSource.Task;
            }

            output.Position = 0;
            return output;
        }
    }
}
