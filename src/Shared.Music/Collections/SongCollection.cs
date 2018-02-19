using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Music.Collections.Models;
using Shared.Music.Processors;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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

        internal async Task<SongData> GetSongAsync(ObjectId songId)
        {
            var result = await collection.FindAsync((f) => f.Id == songId);
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

        internal async Task<Stream> OpenOpusStreamAsync(ObjectId opusId)
        {
            return await opusCollection.OpenOpusStreamAsync(opusId);
        }

        internal async Task<List<SongData>> GetAllAsync()
        {
            var result = await collection.FindAsync(FilterDefinition<SongData>.Empty);
            return await result.ToListAsync();
        }

        // ==========
        // FROM THIS POINT ON, SONGS ARE CREATED VIA PROCESSORS!
        // ==========

        private async Task<bool> DuplicateCheckAsync(ObjectId songId)
        {
            if (await GetSongAsync(songId) != null) return true;
            return false;
        }
        
        internal async Task<ObjectId> DownloadFromYouTubeAsync(string url)
        {
            YouTubeProcessor processor = await YouTubeProcessor.RetrieveAsync(url);
            ObjectId songId = new ObjectId(processor.VideoId);

            if (await DuplicateCheckAsync(songId)) return songId;

            Stream opusStream = await processor.ProcessAudioAsync();
            ObjectId opusId = await opusCollection.StoreOpusStreamAsync($"{songId}.opus", opusStream);
            SongData songData = new SongData(songId, opusId, processor.Metadata);

            await UpdateSongAsync(songData);

            return songId;
        }

        internal async Task<ObjectId> DownloadFromDiscordAsync(string url, string uploader, ulong attachmentId)
        {
            DiscordProcessor processor = await DiscordProcessor.RetrieveAsync(url, uploader);
            ObjectId songId = new ObjectId(attachmentId.ToString());

            if (await DuplicateCheckAsync(songId)) return songId;

            Stream opusStream = await processor.ProcessAudioAsync();
            ObjectId opusId = await opusCollection.StoreOpusStreamAsync($"{songId}.opus", opusStream);
            SongData songData = new SongData(songId, opusId, processor.Metadata);

            await UpdateSongAsync(songData);

            return songId;
        }
    }
}
