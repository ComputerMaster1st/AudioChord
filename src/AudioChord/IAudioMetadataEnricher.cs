using System.Threading.Tasks;

namespace AudioChord
{
    /// <summary>
    /// Alter or Add more metadata after the song has been retrieved
    /// </summary>
    public interface IAudioMetadataEnricher
    {
        Task<ISong> EnrichAsync(ISong song);
    }
}