using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Shared.Music.Collections.Models;
using System;
using System.Threading.Tasks;

namespace Shared.Music.Collections
{
    internal class SongCollection
    {
        private IMongoCollection<Song> collection;
        private GridFSBucket bucket;

        internal SongCollection(IMongoDatabase database)
        {
            collection = database.GetCollection<Song>(typeof(Song).Name);
        }

        internal async Task<Song> GetSongAsync(ObjectId Id)
        {
            var result = await collection.FindAsync((f) => f.Id.Equals(Id));
            return await result.FirstOrDefaultAsync();
        }

        internal async Task UpdateSongAsync(Song song)
        {
            await collection.ReplaceOneAsync((f) => f.Id.Equals(song.Id), song, new UpdateOptions() { IsUpsert = true });
        }

        internal async Task DeleteSongAsync(ObjectId Id)
        {
            await collection.DeleteOneAsync((f) => f.Id.Equals(Id));
        }
    }
}
