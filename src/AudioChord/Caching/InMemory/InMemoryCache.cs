using System.IO;
using System.Threading.Tasks;

namespace AudioChord.Caching.InMemory
{
    public class InMemoryCache : ISongCache
    {
        public Task<(bool success, Stream result)> TryFindCachedSongAsync(SongId id)
        {
            throw new System.NotImplementedException();
        }

        public Task CacheSongAsync(ISong song)
        {
            throw new System.NotImplementedException();
        }
    }
}