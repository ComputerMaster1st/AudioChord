using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Music.Collections.Models;
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

        internal async Task DeleteSongAsync(ObjectId songId)
        {
            await collection.DeleteOneAsync((f) => f.Id == songId);
        }

        internal async Task<Stream> OpenOpusStreamAsync(ObjectId opusId)
        {
            return await opusCollection.OpenOpusStreamAsync(opusId);
        }

        // ==========
        // FROM THIS POINT ON, SONGS ARE CREATED VIA PROCESSORS!
        // ==========

        private async Task<bool> DuplicateCheckAsync(ObjectId songId)
        {
            if (await GetSongAsync(songId) != null) return false;
            return true;
        }
        
        internal async Task<ObjectId> DownloadFromYouTubeAsync(string url)
        {
            YouTubeProcessor processor = await YouTubeProcessor.RetrieveAsync(url);
            ObjectId songId = ObjectId.Parse(processor.VideoId);

            if (await DuplicateCheckAsync(songId)) return songId;

            Stream opusStream = await processor.ProcessAudioAsync();
            ObjectId opusId = await opusCollection.StoreOpusStreamAsync($"{songId}.opus", opusStream);
            SongData songData = new SongData(songId, opusId, processor.Metadata);

            await UpdateSongAsync(songData);

            return songId;
        }
    }
}
