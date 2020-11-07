using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AudioChord.Caching;
using AudioChord.Exceptions;
using AudioChord.Extractors;
using AudioChord.Metadata;

namespace AudioChord.Collections
{
    /// <summary>
    /// Interface between the MongoDB database and the songs
    /// </summary>
    internal class SongCollection
    {
        // private readonly IMongoCollection<SongInformation> _collection;
        private readonly IMetadataProvider _provider;
        private readonly ISongCache _cache;

        internal SongCollection(IMetadataProvider provider, ISongCache cache)
        {
            // _collection = database.GetCollection<SongInformation>("SongData");
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        internal async Task<ISong?> TryGetSongAsync(SongId id)
        {
            SongMetadata? metadata = await _provider.GetSongMetadataAsync(id);

            if (metadata is null)
                // We have no metadata for this song!, assume it doesn't exist
                return default;

            // Since song id's are meant to be unique we can expect exactly one song
            (bool isCached, Stream stream) = await _cache.TryFindCachedSongAsync(id);

            return isCached
                ? new Song(id, metadata, stream)
                : default;
        }

        internal async Task<IEnumerable<SongId>> GetRandomSongs(long amount)
        {
            // If the provider has an implementation for random selections use that
            if (_provider is ISupportsRandomMetadataRetrieval retriever) 
                return await retriever.GetRandomSongMetadataAsync(amount)
                    .ToListAsync();
            else
            {
                // Use a less efficient implementation for retrieving random songs
                List<SongMetadata> allSongIds = await _provider.GetAllMetadataAsync()
                    .ToListAsync();
                
                // Shuffle all metadata
                allSongIds.Shuffle(new Random());
                
                // Shrink the list to the required size if needed
                if (allSongIds.Count > amount)
                    allSongIds.RemoveRange((int)amount - 1, (int)amount);

                return allSongIds
                    .Select(song => song.Id);
            }
        }

        /// <summary>
        /// Check if a song already exists in the database
        /// </summary>
        /// <param name="id">The <see cref="SongId"/> to look for</param>
        /// <returns><see langword="true"/> if the song already exists in the database</returns>
        internal async Task<bool> CheckAlreadyExistsAsync(SongId id) 
            => await _provider.GetSongMetadataAsync(id) != null;

        /// <summary>
        /// Store a song in the database
        /// </summary>
        /// <param name="song">The <see cref="ISong"/> to store in the database</param>
        /// <returns>a <see cref="Song"/>that is the exact representation of the original song, but stored in the database</returns>
        public async Task<ISong> StoreSongAsync(ISong song)
        {
            // Do not save "nothing" to the database
            if ((await song.GetMusicStreamAsync()).Length <= 0)
                throw new InvalidOperationException($"Attempted to save song '{song.Metadata.Id}' to the cache while the stream length was 0!");

            // WARNING: This operation is NOT atomic and can result in GridFS files without SongData (if interrupted)
            // MongoDB transactions only works on clusters, and single node clusters are not recommended for production
            await _cache.CacheSongAsync(song);
            await _provider.StoreSongMetadataAsync(song);

            ISong? retrieved = await TryGetSongAsync(song.Metadata.Id);
            
            // Replace the song with the song from the database
            return retrieved ?? throw new SongNotFoundException($"Could not find '{song.Metadata.Id}' in the database");
        }

        // ==========
        // FROM THIS POINT ON, SONGS ARE CREATED VIA PROCESSORS!
        // ==========

        /// <summary>
        /// Check if the song exists in the database. If not download, store and return the song
        /// </summary>
        /// <param name="url">The url to attempt to locate</param>
        /// <param name="configuration">configuration for the parser</param>
        /// <returns>A located or downloaded youtube <see cref="ISong"/></returns>
        internal async Task<ISong> DownloadFromYouTubeAsync(string url, ExtractorConfiguration configuration)
        {
            // Build the corresponding id
            string youtubeVideoId = YoutubeExplode.Videos.VideoId.TryParse(url);
            SongId id = new SongId(YouTubeExtractor.ProcessorPrefix, youtubeVideoId);

            // Check if the song is already cached, the same youtube video can be downloaded twice
            ISong? retrieved = await TryGetSongAsync(id);
            if (retrieved != null)
                return retrieved;

            // Not in the cache, extract the audio from the url
            IAudioExtractor extractor = configuration.ImportedHttpClient == null ? new YouTubeExtractor() : new YouTubeExtractor(configuration.ImportedHttpClient);
            ISong directlyDownloaded = await extractor.ExtractAsync(url, configuration);

            // Cache the song so that we do not need to download it again
            return await StoreSongAsync(directlyDownloaded);
        }
    }
}