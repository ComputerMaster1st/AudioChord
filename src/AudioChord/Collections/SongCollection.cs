using AudioChord.Collections.Models;
using AudioChord.Processors;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
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

        private readonly Func<DatabaseSong, Task<Stream>> retrieveSongStreamFunction;

        internal SongCollection(IMongoDatabase database)
        {
            collection = database.GetCollection<SongData>(typeof(SongData).Name);

            retrieveSongStreamFunction = OpenOpusStreamAsync;

            opusCollection = new OpusCollection(database);
        }

        internal async Task<ISong> GetSongAsync(string songId)
        {
            SongData song = await FindSongAsync(songId);

            if (song == null) return null;
            else if (song.LastAccessed < DateTime.Now.AddDays(-90))
            {
                await DeleteSongAsync(song);
                return null;
            }

            return new DatabaseSong(song.Id, song.Metadata, retrieveSongStreamFunction);
        }

        private async Task<SongData> FindSongAsync(string Id)
        {
            var result = await collection.FindAsync((f) => f.Id == Id);
            return await result.FirstOrDefaultAsync();
        }

        internal async Task UpdateSongAsync(SongData song)
        {
            await collection.ReplaceOneAsync((f) => f.Id == song.Id, song, new UpdateOptions() { IsUpsert = true });
        }

        internal async Task DeleteSongAsync(SongData song)
        {
            await collection.DeleteOneAsync((f) => f.Id == song.Id);
            await opusCollection.DeleteAsync(song.OpusId);
        }

        internal async Task<IEnumerable<ISong>> GetAllAsync()
        {
            return (await collection.FindAsync(FilterDefinition<SongData>.Empty))
                .ToList()
                .ConvertAll(new Converter<SongData, ISong>(
                    (songData) =>
                    {
                        return new DatabaseSong(songData.Id, songData.Metadata, retrieveSongStreamFunction);
                    }));
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
            SongData songData = await FindSongAsync(song.Id);

            //update the last-used timestamp
            songData.LastAccessed = DateTime.Now;
            await UpdateSongAsync(songData);

            //give the stream back
            return await opusCollection.OpenOpusStreamAsync(songData.OpusId);
        }

        /// <summary>
        /// Store a song in the database
        /// </summary>
        /// <param name="song">The song to store in the database</param>
        /// <returns>a <see cref="DatabaseSong"/>that is the exact representation of the original song, but stored in the database</returns>
        private async Task<DatabaseSong> StoreSongAsync(ISong song)
        {
            if (song is DatabaseSong)
                //the song has already been stored in the database
                return (DatabaseSong)song;

            ObjectId opusId = await opusCollection.StoreOpusStreamAsync($"{song.Id}.opus", await song.GetMusicStreamAsync());
            SongData songData = new SongData(song.Id, opusId, song.Metadata);
            await UpdateSongAsync(songData);

            //replace the song with the song from the database
            return new DatabaseSong(song.Id, song.Metadata, retrieveSongStreamFunction);
        }

        // ==========
        // FROM THIS POINT ON, SONGS ARE CREATED VIA PROCESSORS!
        // ==========
        
        internal async Task<ISong> DownloadFromYouTubeAsync(Uri videoLocation)
        {
            YouTubeProcessor processor = new YouTubeProcessor();
            ISong opusSong = await processor.ExtractSongAsync(videoLocation);

            return await StoreSongAsync(opusSong);
        }

        internal async Task<ISong> DownloadFromYouTubeAsync(string videoId)
        {
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
