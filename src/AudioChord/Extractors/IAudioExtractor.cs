using System.Threading.Tasks;

namespace AudioChord.Extractors
{
    public interface IAudioExtractor
    {
        Task<ISong> ExtractAsync(string url, ExtractorConfiguration configuration);
    }
}