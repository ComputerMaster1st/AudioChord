using System.Threading.Tasks;

namespace AudioChord.Extractors
{
    public interface IAudioExtractor
    {
        /// <summary>
        /// Check if this extractor can extract the audio from the given Uri
        /// </summary>
        /// <param name="url">The Uri that we want to extract from</param>
        /// <returns>true if the extractor knows how to extract audio on that location</returns>
        bool CanExtract(string url);

        /// <summary>
        /// Try to create a SongId from the given url
        /// </summary>
        /// <param name="url">the url to extract an <see cref="SongId"/> from</param>
        /// <param name="id">the successfully converted SongId</param>
        /// <returns>true if the conversions has succeeded</returns>
        bool TryExtractSongId(string url, out SongId id);
        
        Task<ISong> ExtractAsync(string url, ExtractorConfiguration configuration);
    }
}