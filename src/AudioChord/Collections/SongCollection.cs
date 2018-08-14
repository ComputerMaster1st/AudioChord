using AudioChord.Collections.Models;
using AudioChord.Processors;
using MongoDB.Bson;
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

        internal SongCollection(IMongoDatabase database)
        {
            collection = database.GetCollection<SongData>(typeof(SongData).Name);
            opusCollection = new OpusCollection(database);
        }

        internal async Task<ISong> GetSongAsync(string songId)
        {
            // First cleanup the collection before querying
            await DeleteExpiredSongsAsync();

            SongData song = await FindSongAsync(songId);

            if (song is null)
                throw new ArgumentException($"The song-id '{songId}' was not found in the database");

            return new DatabaseSong(SongId.Parse(song.Id), song.Metadata, OpenOpusStreamAsync);
        }

        private async Task DeleteExpiredSongsAsync()
        {
            // First find all the songs that we wan to delete
            var songs = await (await collection
                .FindAsync(filter => filter.LastAccessed < DateTime.Now.AddDays(-90)))
                .ToListAsync();

            // Delete the metadata information first so that we can't open half-deleted songs
            await collection
                .DeleteManyAsync(filter => filter.LastAccessed < DateTime.Now.AddDays(-90));

            // Delete the actual opus data
            foreach (SongData data in songs)
            {
                // WARNING: This operation is NOT atomic and can result in half-deleted songs if they are stretched over multiple documents in GridFS
                // TODO: Make an atomic storage system for opus data of songs
                await opusCollection.DeleteAsync(data.OpusId);
            }
        }

        private async Task<SongData> FindSongAsync(string Id)
        {
            var result = await collection.FindAsync((f) => f.Id == Id);
            return await result.FirstOrDefaultAsync();
        }

        internal Task UpdateSongAsync(SongData song)
        {
            return collection.ReplaceOneAsync((f) => f.Id == song.Id, song, new UpdateOptions() { IsUpsert = true });
        }

        internal Task DeleteSongAsync(SongData song)
        {
            return Task.WhenAll(collection.DeleteOneAsync(f => f.Id == song.Id), opusCollection.DeleteAsync(song.OpusId));
        }

        internal async Task<IEnumerable<ISong>> GetAllAsync()
        {
            return (await collection.FindAsync(FilterDefinition<SongData>.Empty))
                .ToList()
                .ConvertAll(new Converter<SongData, ISong>(
                    (songData) =>
                    {
                        return new DatabaseSong(SongId.Parse(songData.Id), songData.Metadata, OpenOpusStreamAsync);
                    }));
        }

        /// <summary>
        /// Check if a song already exists in the database
        /// </summary>
        /// <param name="location">The source of the song</param>
        /// <returns><see langword="true"/> if the song already exists in the database</returns>
        internal async Task<bool> CheckAlreadyExistsAsync(string id)
        {
            return (await collection.FindAsync(filter => filter.Id == id)).Any();
        }

        internal Task<double> GetTotalBytesUsedAsync() => opusCollection.TotalBytesUsedAsync();

        //internal async Task<int> ResyncDatabaseAsync()
        //{
        //    int deletedDesyncedFiles = 0;

        //    List<SongData> songList = await GetAllAsync();

        //    List<ObjectId> listedOpusIds = new List<ObjectId>();
        //    IEnumerable<ObjectId> allOpusIds = await opusCollection.GetAllOpusIdsAsync();

        //    foreach (SongData data in songList) listedOpusIds.Add(data.OpusId);

        //    foreach (ObjectId opusId in allOpusIds)
        //    {
        //        if (!listedOpusIds.Contains(opusId))
        //        {
        //            await opusCollection.DeleteAsync(opusId);
        //            deletedDesyncedFiles++;
        //        }
        //    }

        //    return deletedDesyncedFiles;
        //}

        /// <summary>
        /// Open a stream for the song stored in the database
        /// </summary>
        /// <param name="song">The song to open a stream for</param>
        /// <returns>A stream with opus data to send to discord</returns>
        private async Task<Stream> OpenOpusStreamAsync(DatabaseSong song)
        {
            //retrieve the requested song
            SongData songData = await FindSongAsync(song.Id.ToString());

            //update the last-used timestamp
            songData.LastAccessed = DateTime.Now;
            await UpdateSongAsync(songData);

            //give the stream back
            return await opusCollection.OpenOpusStreamAsync(songData.OpusId);
        }

        /// <summary>
        /// Store a song in the database
        /// </summary>
        /// <param name="song">The <see cref="ISong"/> to store in the database</param>
        /// <returns>a <see cref="DatabaseSong"/>that is the exact representation of the original song, but stored in the database</returns>
        private async Task<DatabaseSong> StoreSongAsync(ISong song)
        {
            if (song is DatabaseSong)
                //the song has already been stored in the database
                return (DatabaseSong)song;

            ObjectId opusId = await opusCollection.StoreOpusStreamAsync($"{song.Id}.opus", await song.GetMusicStreamAsync());
            SongData songData = new SongData(song.Id.ToString(), opusId, song.Metadata);
            await UpdateSongAsync(songData);

            //replace the song with the song from the database
            return new DatabaseSong(song.Id, song.Metadata, OpenOpusStreamAsync);
        }

        // ==========
        // FROM THIS POINT ON, SONGS ARE CREATED VIA PROCESSORS!
        // ==========

        internal async Task<ISong> DownloadFromYouTubeAsync(string videoId)
        {
            //check if the song is already in the database
            if (await CheckAlreadyExistsAsync(videoId))
                return await GetSongAsync(videoId);

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
