using MongoDB.Driver;
using Shared.Music.Collections.Models;
using System;
using System.Threading.Tasks;

namespace Shared.Music.Collections
{
    internal class PlaylistCollection
    {
        private IMongoCollection<Playlist> Collection;

        internal PlaylistCollection(IMongoCollection<Playlist> Collection)
        {
            this.Collection = Collection;
        }

        internal async Task<Guid> CreateAsync()
        {
            Playlist playlist = new Playlist();
            await Collection.InsertOneAsync(playlist);
            return playlist.Id;
        }

        internal async Task<Playlist> GetAsync(Guid Id)
        {
            var Result = await Collection.FindAsync((f) => f.Id.Equals(Id));
            return await Result.FirstOrDefaultAsync();
        }

        internal async Task UpdateAsync(Guid PlaylistId, Playlist Playlist)
        {
            await Collection.ReplaceOneAsync((f) => f.Id.Equals(PlaylistId), Playlist);
        }

        internal async Task DeleteAsync(Guid Id)
        {
            await Collection.DeleteOneAsync((f) => f.Id.Equals(Id));
        }
    }
}
