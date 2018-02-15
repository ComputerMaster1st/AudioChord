using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Shared.Music.Processors
{
    public class FFMpegEncoder
    {
        ProcessStartInfo ffmpegProcessInfo;

        public FFMpegEncoder(string filepath)
        {
            ffmpegProcessInfo = new ProcessStartInfo()
            {
                FileName = @"C:\Users\ComputerMaster1st\Downloads\ffmpeg-20180215-fb58073-win64-static\bin\ffmpeg.exe", //don't know how concentus will react to different bitrate (was 96k)
                Arguments = $"-i {filepath} -ar 48k -codec:a libopus -b:a 128k -ac 2 -f opus pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = false
            };
        }

        public Task<Stream> ProcessAsync()
        {
            TaskCompletionSource<Stream> taskCompletionSource = new TaskCompletionSource<Stream>();
            MemoryStream stream = new MemoryStream();

            //create a new process for ffmpeg
            Process process = new Process()
            {
                StartInfo = ffmpegProcessInfo,
                EnableRaisingEvents = true
            };

            void OutputDelegate(object sender, DataReceivedEventArgs args)
            {
                process.OutputDataReceived -= OutputDelegate;
                process.StandardOutput.BaseStream.CopyTo(stream);
            }

            process.OutputDataReceived += OutputDelegate;

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
