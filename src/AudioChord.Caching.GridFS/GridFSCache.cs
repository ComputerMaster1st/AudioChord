using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System.IO;
using System.Threading.Tasks;


namespace AudioChord.Caching.GridFS
{
    /// <summary>
    /// Caches songs to a mongodb GridFS Filesystem
    /// </summary>
    public class GridFSCache : ISongCache
    {
        private GridFSCacheCleaner cleaner;
        protected IGridFSBucket<string> cache;

        /// <summary>
        /// Create a new Cache using the <paramref name="cache"/> bucket
        /// </summary>
        /// <param name="cache"></param>
        public GridFSCache(IGridFSBucket<string> cache)
        {
            this.cache = cache;
            cleaner = new GridFSCacheCleaner(cache);
        }

        /// <summary>
        /// Cache the <see cref="ISong"/> in the GridFS cache if it doesn't already exist
        /// </summary>
        /// <param name="song">The <see cref="ISong"/> to try to cache</param>
        public async Task CacheSongAsync(ISong song)
        {
            // Clean the cache first
            await cleaner.CleanExpiredCacheEntries();

            // We do not need to upload songs to the cache if they already exist
            if (!DoesSongIdExist(song.Id))
            {
                await cache
                    .UploadFromStreamAsync(
                        id: song.Id.ToString(),
                        filename: $"{song.Id}.opus",
                        source: await song.GetMusicStreamAsync(),
                        options: new GridFSUploadOptions() { Metadata = cleaner.GenerateGridFSMetadata() }
                    );
            }
        }

        public async Task<(bool, Stream)> TryFindCachedSongAsync(SongId id)
        {
            // Check if we have the song in the cache
            if (DoesSongIdExist(id))
            {
                // Update the timestamp of the cache item
                cleaner.UpdateCacheTimestamp(id);

                // Return the cached Stream
                return (true, Stream.Synchronized(await cache.OpenDownloadStreamAsync(id.ToString())));
            }
            else
            {
                return (false, null);
            }
        }

        /// <summary>
        /// Validate if the <see cref="SongId"/> Exists in the GridFS Cache
        /// </summary>
        /// <param name="id">The <see cref="SongId"/> to validate</param>
        /// <returns>If the <see cref="SongId"/> exists in the GridFS Cache</returns>
        private bool DoesSongIdExist(SongId id)
        {
            FilterDefinitionBuilder<GridFSFileInfo<string>> builder = new FilterDefinitionBuilder<GridFSFileInfo<string>>();
            var definition = builder.Where(filter => filter.Id == id.ToString());

            return cache.Find(definition).Any();
        }
    }
}
