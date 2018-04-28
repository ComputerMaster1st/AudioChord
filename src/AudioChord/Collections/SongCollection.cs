using MongoDB.Bson;
using MongoDB.Driver;
using AudioChord.Collections.Models;
using AudioChord.Processors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using YoutubeExplode;

namespace AudioChord.Collections
{
    internal class SongCollection
    {
        private IMongoCollection<SongData> collection;
        private OpusCollection opusCollection;

        internal SongCollection(IMongoDatabase database)
        {
            collection = database.GetCollection<SongData>(typeof(SongData).Name);
            opusCollection = new OpusCollection(database);
        }

        internal async Task<SongData> GetSongAsync(string songId)
        {
            var result = await collection.FindAsync((f) => f.Id == songId);
            SongData song = await result.FirstOrDefaultAsync();

            if (song == null) return null;
            else if (song.LastAccessed < DateTime.Now.AddDays(-90))
            {
                await DeleteSongAsync(song);
                return null;
            }

            return song;
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

        internal async Task<Stream> OpenOpusStreamAsync(Song song)
        {
            //retrieve the requested song
            SongData songData = await GetSongAsync(song.Id);

            //update the last-used timestamp
            songData.LastAccessed = DateTime.Now;
            await UpdateSongAsync(songData);

            //give the stream back
            return await opusCollection.OpenOpusStreamAsync(song.opusFileId);
        }

        internal async Task<List<SongData>> GetAllAsync()
        {
            var result = await collection.FindAsync(FilterDefinition<SongData>.Empty);
            return await result.ToListAsync();
        }

        internal async Task<double> GetTotalBytesUsedAsync()
        {
            return await opusCollection.TotalBytesUsedAsync();
        }

        internal async Task<int> ResyncDatabaseAsync()
        {
            int deletedDesyncedFiles = 0;

            List<SongData> songList = await GetAllAsync();

            List<ObjectId> listedOpusIds = new List<ObjectId>();
            IEnumerable<ObjectId> allOpusIds = await opusCollection.GetAllOpusIdsAsync();

            foreach (SongData data in songList) listedOpusIds.Add(data.OpusId);

            foreach (ObjectId opusId in allOpusIds)
            {
                if (!listedOpusIds.Contains(opusId))
                {
                    await opusCollection.DeleteAsync(opusId);
                    deletedDesyncedFiles++;
                }
            }

            return deletedDesyncedFiles;
        }

        // ==========
        // FROM THIS POINT ON, SONGS ARE CREATED VIA PROCESSORS!
        // ==========
        
        internal async Task<string> DownloadFromYouTubeAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException("No url has been provided!");

            if (!YoutubeClient.TryParseVideoId(url, out string videoId))
                throw new ArgumentException("Video Url could not be parsed!");

            SongData songData = await GetSongAsync("YOUTUBE#" + videoId);
            if (songData != null) return songData.Id;

            YouTubeProcessor processor = await YouTubeProcessor.RetrieveAsync(videoId);
            Stream opusStream = await processor.ProcessAudioAsync();

            string songId = "YOUTUBE#" + processor.VideoId;
            ObjectId opusId = await opusCollection.StoreOpusStreamAsync($"{songId}.opus", opusStream);
            songData = new SongData(songId, opusId, processor.Metadata);

            await UpdateSongAsync(songData);

            return songId;
        }

        internal async Task<string> DownloadFromDiscordAsync(string url, string uploader, ulong attachmentId)
        {
            DiscordProcessor processor = await DiscordProcessor.RetrieveAsync(url, uploader);
            Stream opusStream = await processor.ProcessAudioAsync();

            string songId = "DISCORD#" + attachmentId;
            ObjectId opusId = await opusCollection.StoreOpusStreamAsync($"{songId}.opus", opusStream);
            SongData songData = new SongData(songId, opusId, processor.Metadata);

            await UpdateSongAsync(songData);

            return songId;
        }
    }
}
