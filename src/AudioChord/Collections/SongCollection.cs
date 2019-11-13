using AudioChord.Collections.Models;
using AudioChord.Processors;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AudioChord.Caching;
using AudioChord.Exceptions;
using AudioChord.Extractors;
using MongoDB.Driver.Linq;
using YoutubeExplode;

namespace AudioChord.Collections
{
    /// <summary>
    /// Interface between the MongoDB database and the songs
    /// </summary>
    internal class SongCollection
    {
        private readonly IMongoCollection<SongInformation> _collection;
        private readonly ISongCache _cache;

        internal SongCollection(IMongoDatabase database, ISongCache cache)
        {
            _collection = database.GetCollection<SongInformation>("SongData");
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        internal async Task<(bool, ISong)> TryGetSongAsync(SongId id)
        {
            IFindFluent<SongInformation, SongInformation> metadataQuery = _collection
                .Find(filter => Equals(filter.Id, id));

            if (!metadataQuery.Any())
                // We have no metadata for this song!, assume it doesn't exist
                return (false, default);

            // Since song id's are meant to be unique we can expect exactly one song
            SongInformation information = metadataQuery.Single();

            (bool isCached, Stream stream) = await _cache.TryFindCachedSongAsync(information.Id);

            return isCached
                ? (true, new Song(information.Id, information.Metadata, stream))
                : (false, default);
        }

        internal IEnumerable<SongId> GetRandomSongs(long amount)
        {
            return _collection
                .AsQueryable()
                .Sample(amount)
                .Select(info => info.Id)
                .AsEnumerable();
        }

        /// <summary>
        /// Check if a song already exists in the database
        /// </summary>
        /// <param name="id">The <see cref="SongId"/> to look for</param>
        /// <returns><see langword="true"/> if the song already exists in the database</returns>
        internal bool CheckAlreadyExists(SongId id)
        {
            // We are only querying for 1 or 0 documents, it's quicker to use sync (no async overhead)
            return _collection
                .Find(filter => Equals(filter.Id, id))
                .Any();
        }

        /// <summary>
        /// Store a song in the database
        /// </summary>
        /// <param name="song">The <see cref="ISong"/> to store in the database</param>
        /// <returns>a <see cref="DatabaseSong"/>that is the exact representation of the original song, but stored in the database</returns>
        private async Task<ISong> StoreSongAsync(ISong song)
        {
            // The song has already been stored in the database
            if (song is DatabaseSong databaseSong)
                return databaseSong;

            // Do not save "nothing" to the database
            if ((await song.GetMusicStreamAsync()).Length <= 0)
                throw new InvalidOperationException($"Attempted to save song '{song.Id}' to the cache while the stream length was 0!");

            // WARNING: This operation is NOT atomic and can result in GridFS files without SongData (if interrupted)
            // MongoDB transactions only works on clusters, and single node clusters are not recommended for production
            await _cache.CacheSongAsync(song);
            await _collection.InsertOneAsync(new SongInformation(song.Id, song.Metadata));

            (bool isSuccess, ISong found) = await TryGetSongAsync(song.Id);

            // Replace the song with the song from the database
            if (isSuccess)
                return found;

            throw new SongNotFoundException($"Could not find '{song.Id}' in the database");
        }

        #region Metadata Queries

        internal async Task<IEnumerable<SongMetadata>> EnumerateMetadataAsync()
        {
            return (await _collection.FindAsync(FilterDefinition<SongInformation>.Empty))
                .ToEnumerable()
                .Select((source, index) => source.Metadata);
        }

        internal SongMetadata TryFindSongMetadata(SongId id)
        {
            return _collection
                .Find(filter => Equals(filter.Id, id))
                .ToEnumerable()
                .Select(information => information.Metadata)
                .FirstOrDefault();
        }

        #endregion

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
            string youtubeVideoId = YoutubeClient.ParseVideoId(url);
            SongId id = new SongId(YouTubeExtractor.ProcessorPrefix, youtubeVideoId);

            // Check if the song is already cached, the same youtube video can be downloaded twice
            (bool isCached, ISong song) = await TryGetSongAsync(id);
            if (isCached)
                return song;

            // Not in the cache, extract the audio from the url
            IAudioExtractor extractor = new YouTubeExtractor();
            ISong directlyDownloaded = await extractor.ExtractAsync(url, configuration);

            // Cache the song so that we do not need to download it again
            return await StoreSongAsync(directlyDownloaded);
        }

        internal async Task<ISong> DownloadFromDiscordAsync(string url, string uploader, ulong attachmentId)
        {
            DiscordProcessor processor = await DiscordProcessor.RetrieveAsync(url, uploader, attachmentId);
            Song opusSong = await processor.ProcessAudioAsync();

            return await StoreSongAsync(opusSong);
        }
    }
}