using AudioChord.Collections.Models;
using AudioChord.Processors;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AudioChord.Collections
{
    /// <summary>
    /// Interface between the MongoDB database and the songs
    /// </summary>
    internal class SongCollection
    {
        private IMongoCollection<SongData> collection;
        private OpusCollection opusCollection;

        private readonly DateTime resyncDate = DateTime.Now;

        internal SongCollection(IMongoDatabase database)
        {
            collection = database.GetCollection<SongData>(typeof(SongData).Name);
            opusCollection = new OpusCollection(database);
        }

        internal async Task<ISong> GetSongAsync(SongId id)
        {
            // First cleanup the collection before querying
            await DeleteExpiredSongsAsync();

            SongData result = collection
                .Find(filter => filter.Id == id)
                .FirstOrDefault() ?? throw new ArgumentException($"The song-id '{id}' was not found in the database");

            return new DatabaseSong(result.Id, result.Metadata, OpenOpusStreamAsync);
        }

        internal async Task<bool> TryGetSongAsync(SongId id, Action<ISong> value)
        {
            // First cleanup the collection before querying
            await DeleteExpiredSongsAsync();

            SongData result = collection
                .Find(filter => filter.Id == id)
                .FirstOrDefault();

            if(result is null)
            {
                value(null);
                return false;
            }
            else
            {
                value(new DatabaseSong(result.Id, result.Metadata, OpenOpusStreamAsync));
                return true;
            }
        }

        private async Task DeleteExpiredSongsAsync()
        {
            // First find all the songs that we want to delete
            var songs = await (await collection
                .FindAsync(filter => filter.LastAccessed < DateTime.Now.AddMonths(-3)))
                .ToListAsync();

            // Delete the metadata information first so that we can't open half-deleted songs
            await collection
                .DeleteManyAsync(filter => filter.LastAccessed < DateTime.Now.AddMonths(-3));

            // Delete the actual opus data
            foreach (SongData data in songs)
            {
                // WARNING: This operation is NOT atomic and can result in half-deleted songs if they are stretched over multiple documents in GridFS
                // TODO: Make an atomic storage system for opus data of songs
                await opusCollection.DeleteAsync(data.Id);
            }
        }

        private async Task ResyncAsync()
        {
            // First check if it's later than 24 hours since the last resync has passed
            if (!(DateTime.Now.Subtract(TimeSpan.FromHours(24)) > resyncDate))
                return;

            // If the amount of documents is more than the amount of GridFS files then there's desync
            if(opusCollection.SongCount > collection.CountDocuments(FilterDefinition<SongData>.Empty))
            {
                List<string> deletionList = new List<string>();

                foreach(string id in await opusCollection.GetAllIdsAsync())
                {
                    SongId parsedId = SongId.Parse(id);

                    // delete the GridFS file if no matching SongData document is found
                    if (!collection.Find(filter => filter.Id == parsedId).Any())
                        await opusCollection.DeleteAsync(parsedId);
                }
            }
        }

        internal Task UpdateExpirationAsync(SongId id)
        {
            SongData result = collection
                .Find(filter => filter.Id == id)
                .FirstOrDefault() ?? throw new ArgumentException($"The song-id '{id}' was not found in the database");

            return collection.ReplaceOneAsync(filter => filter.Id == result.Id, result);
        }

        internal Task DeleteSongAsync(ISong song)
        {
            return Task.WhenAll(collection.DeleteOneAsync(filter => filter.Id == song.Id), opusCollection.DeleteAsync(song.Id));
        }

        internal async Task<IEnumerable<ISong>> GetAllAsync()
        {
            return (await collection.FindAsync(FilterDefinition<SongData>.Empty))
                .ToList()
                .ConvertAll(new Converter<SongData, DatabaseSong>(target => { return new DatabaseSong(target.Id, target.Metadata, OpenOpusStreamAsync); }));
        }

        /// <summary>
        /// Check if a song already exists in the database
        /// </summary>
        /// <param name="id">The <see cref="SongId"/> to look for</param>
        /// <returns><see langword="true"/> if the song already exists in the database</returns>
        internal bool CheckAlreadyExists(SongId id)
        {
            // We are only querying for 1 or 0 documents, it's quicker to use sync (no async overhead)
            return collection
                .Find(filter => filter.Id == id)
                .Any();
        }

        internal Task<double> GetTotalBytesUsedAsync() => opusCollection.TotalBytesUsedAsync();

        /// <summary>
        /// Open a stream for the song stored in the database
        /// </summary>
        /// <param name="song">The song to open a stream for</param>
        /// <returns>A stream with opus data to send to discord</returns>
        private async Task<Stream> OpenOpusStreamAsync(DatabaseSong song)
        {
            // Update the last-used timestamp
            await UpdateExpirationAsync(song.Id);

            // Give the stream back
            return await opusCollection.OpenOpusStreamAsync(song);
        }

        /// <summary>
        /// Store a song in the database
        /// </summary>
        /// <param name="song">The <see cref="ISong"/> to store in the database</param>
        /// <returns>a <see cref="DatabaseSong"/>that is the exact representation of the original song, but stored in the database</returns>
        private async Task<DatabaseSong> StoreSongAsync(ISong song)
        {
            if (song is DatabaseSong databaseSong)
                //the song has already been stored in the database
                return databaseSong;

            // WARNING: This operation is NOT atomic and can result in GridFS files without SongData (if interrupted)
            // TODO: Fix desyncing using mongodb transactions if possible
            await opusCollection.StoreOpusStreamAsync(song);
            await collection.InsertOneAsync(new SongData(song.Id, song.Metadata));

            //replace the song with the song from the database
            return new DatabaseSong(song.Id, song.Metadata, OpenOpusStreamAsync);
        }

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

            //check if the song is already in the database
            if (CheckAlreadyExists(id))
                return await GetSongAsync(id);

            YouTubeProcessor processor = new YouTubeProcessor();
            ISong opusSong = await processor.ExtractSongAsync(videoId);

            return await StoreSongAsync(opusSong);
        }

        internal async Task<ISong> DownloadFromDiscordAsync(string url, string uploader, ulong attachmentId)
        {
            DiscordProcessor processor = await DiscordProcessor.RetrieveAsync(url, uploader, attachmentId);
            Song opusSong = await processor.ProcessAudioAsync();

            return await StoreSongAsync(opusSong);
        }
    }
}
