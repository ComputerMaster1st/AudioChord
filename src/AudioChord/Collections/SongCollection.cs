using AudioChord.Collections.Models;
using AudioChord.Processors;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AudioChord.Caching;
using MongoDB.Driver.Linq;

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
            SongInformation information = _collection
                .Find(filter => Equals(filter.Id, id))
                .FirstOrDefault();

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
            if (song is DatabaseSong databaseSong)
                // The song has already been stored in the database
                return databaseSong;

            // WARNING: This operation is NOT atomic and can result in GridFS files without SongData (if interrupted)
            // TODO: Fix synchronization issues using mongodb transactions if possible

            using (IClientSessionHandle handle = _collection.Database.Client.StartSession())
            {
                handle.StartTransaction();
                
                try
                {
                    await _cache.CacheSongAsync(song);
                    await _collection.InsertOneAsync(handle, new SongInformation(song.Id, song.Metadata));
                    handle.CommitTransaction();
                }
                catch (Exception)
                {
                    handle.AbortTransaction();
                    throw;
                }
            }

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
                .Find(filter => filter.Id == id)
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
        /// <param name="videoId"></param>
        /// <returns></returns>
        internal async Task<ISong> DownloadFromYouTubeAsync(string videoId)
        {
            // Build the corresponding id
            SongId id = new SongId(YouTubeProcessor.ProcessorPrefix, videoId);

            // Check if the song is already cached, the same youtube video can be downloaded twice
            (bool isCached, ISong song) = await TryGetSongAsync(id);
            if(isCached)
                return song;
            
            YouTubeProcessor processor = new YouTubeProcessor();
            ISong directlyDownloaded = await processor.ExtractSongAsync(videoId);

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
