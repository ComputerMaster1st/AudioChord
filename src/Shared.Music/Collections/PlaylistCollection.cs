using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Music.Collections.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shared.Music.Collections
{
    internal class PlaylistCollection
    {
        private IMongoCollection<Playlist> collection;

        internal PlaylistCollection(IMongoDatabase database)
        {
            collection = database.GetCollection<Playlist>(typeof(Playlist).Name);
        }

        internal async Task<Playlist> Create()
        {
            Playlist playlist = new Playlist(this);
            await UpdateAsync(playlist);
            return playlist;
        }

        internal async Task<Playlist> GetPlaylistAsync(ObjectId playlistId)
        {
            var result = await collection.FindAsync((f) => f.Id == playlistId);
            Playlist playlist = await result.FirstOrDefaultAsync();
            playlist.collection = this;
            return playlist;
        }

        internal async Task UpdateAsync(Playlist playlist)
        {
            await collection.ReplaceOneAsync((f) => f.Id == playlist.Id, playlist, new UpdateOptions() { IsUpsert = true });
        }

        internal async Task DeleteAsync(ObjectId id)
        {
            await collection.DeleteOneAsync((f) => f.Id == id);
        }

        internal async Task<List<Playlist>> GetAllAsync()
        {
            var result = await collection.FindAsync(FilterDefinition<Playlist>.Empty);
            return await result.ToListAsync();
        }
    }
}
