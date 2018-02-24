using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Music.Collections.Models;
using Shared.Music.Processors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using YoutubeExplode;

namespace Shared.Music.Collections
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

        internal async Task<Stream> OpenOpusStreamAsync(ObjectId opusId)
        {
            return await opusCollection.OpenOpusStreamAsync(opusId);
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

        // ==========
        // FROM THIS POINT ON, SONGS ARE CREATED VIA PROCESSORS!
        // ==========

        private async Task<bool> DuplicateCheckAsync(string songId)
        {
            if (await GetSongAsync(songId) != null) return true;
            return false;
        }
        
        internal async Task<string> DownloadFromYouTubeAsync(string url)
        {
            if (!YoutubeClient.TryParseVideoId(url, out string videoId))
                throw new ArgumentException("Video Url could not be parsed!");

            string songId = $"YOUTUBE#{videoId}";

            if (await DuplicateCheckAsync(songId)) return songId;

            YouTubeProcessor processor = await YouTubeProcessor.RetrieveAsync(videoId);

            Stream opusStream = await processor.ProcessAudioAsync();
            ObjectId opusId = await opusCollection.StoreOpusStreamAsync($"{songId}.opus", opusStream);
            SongData songData = new SongData(songId, opusId, processor.Metadata);

            await UpdateSongAsync(songData);

            return songId;
        }

        internal async Task<string> DownloadFromDiscordAsync(string url, string uploader, ulong attachmentId)
        {
            DiscordProcessor processor = await DiscordProcessor.RetrieveAsync(url, uploader);
            string songId = $"DISCORD#{attachmentId}";

            if (await DuplicateCheckAsync(songId)) return songId;

            Stream opusStream = await processor.ProcessAudioAsync();
            ObjectId opusId = await opusCollection.StoreOpusStreamAsync($"{songId}.opus", opusStream);
            SongData songData = new SongData(songId, opusId, processor.Metadata);

            await UpdateSongAsync(songData);

            return songId;
        }
    }
}
