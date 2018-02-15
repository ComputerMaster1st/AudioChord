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
                FileName = @"C:\Users\Mr_MA\Downloads\ffmpeg-20180215-fb58073-win64-static\bin\ffmpeg.exe", //don't know how concentus will react to different bitrate (was 96k)
                Arguments = $"-i {filepath} -ar 48k -codec:a libopus -b:a 128k -ac 2 -f opus pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = false
            };
        }

        public async Task<MemoryStream> ProcessAsync()
        {
            MemoryStream stream = new MemoryStream();

            //create a new process for ffmpeg
            Process process = new Process()
            {
                StartInfo = ffmpegProcessInfo
            };

            process.Start();
            await process.StandardOutput.BaseStream.CopyToAsync(stream);
            stream.Position = 0;
            process.WaitForExit();

            return stream;
        }
    }
}
