using System;
using System.IO;
using System.Threading.Tasks;

namespace AudioChord.Caching
{
    /// <summary>
    /// A service for caching downloaded songs
    /// </summary>
    public interface ISongCache
    {
        Task<bool> TryFindCachedSongAsync(SongId id, Action<Stream> result);
        Task CacheSongAsync(ISong song);
    }
}