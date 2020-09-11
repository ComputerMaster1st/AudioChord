using System.IO;
using System.Threading.Tasks;

namespace AudioChord.Caching
{
    /// <summary>
    /// A service for caching downloaded songs
    /// </summary>
    public interface ISongCache
    {
        Task<(bool success, Stream result)> TryFindCachedSongAsync(SongId id);
        Task CacheSongAsync(ISong song);
    }
}