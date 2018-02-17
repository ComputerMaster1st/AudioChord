using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Shared.Music.Processors
{
    public class FFMpegEncoder
    {
        private ProcessStartInfo CreateEncoderInfo(string filePath, bool redirectInput = false)
        {
            return new ProcessStartInfo()
            {
                FileName = @"C:\Users\Mr_MA\Downloads\ffmpeg-20180215-fb58073-win64-static\bin\ffmpeg.exe", //don't know how concentus will react to different bitrate (was 96k)
                Arguments = $"-i {filePath} -ar 48k -codec:a libopus -b:a 128k -ac 2 -f opus pipe:1",
                UseShellExecute = false,
                RedirectStandardInput = redirectInput,
                RedirectStandardOutput = true
            };
        }

        public async Task<MemoryStream> ProcessAsync(Stream input)
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
                process.Exited += (obj, args) =>
                {
                    awaitExitSource.SetResult(process.ExitCode);
                };

                process.Start();

                await Task.WhenAny(input.CopyToAsync(process.StandardInput.BaseStream), process.StandardOutput.BaseStream.CopyToAsync(output));

                process.StandardInput.Close();
                int exitCode = await awaitExitSource.Task;
            }

            output.Position = 0;
            return output;
        }

        public async Task<MemoryStream> ProcessAsync(string filePath)
        {
            MemoryStream output = new MemoryStream();
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
                await process.StandardOutput.BaseStream.CopyToAsync(output);
                output.Position = 0;

                //wait for the exit event to happen
                int exitCode = await awaitExitSource.Task;
            }

            return output;
        }
    }
}
