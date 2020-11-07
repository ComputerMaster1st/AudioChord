using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AudioChord.Caching.InMemory
{
    /// <summary>
    /// A Memory-based cache. This can be used for testing purposes or as a temporary solution
    /// </summary>
    public class InMemoryCache : ISongCache
    {
        private Dictionary<SongId, Stream> _cache = new Dictionary<SongId, Stream>();
        
        public Task<(bool success, Stream result)> TryFindCachedSongAsync(SongId id)
        {
            if (_cache.TryGetValue(id, out Stream stream))
                return Task.FromResult((true, stream));

            return Task.FromResult<(bool success, Stream result)>((false, Stream.Null));
        }

        public async Task CacheSongAsync(ISong song)
        {
            _cache.Add(song.Metadata.Id, await song.GetMusicStreamAsync());
        }
    }
}