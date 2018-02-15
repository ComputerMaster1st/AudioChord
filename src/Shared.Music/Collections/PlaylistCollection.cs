using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Music.Collections.Models;
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

        private Playlist CreateAsync()
        {
            Playlist playlist = new Playlist(this);
            return playlist;
        }

        internal async Task<Playlist> GetAsync(ObjectId PlaylistId)
        {
            var Result = await Collection.FindAsync((f) => f.Id.Equals(PlaylistId));
            Playlist playlist = ((await Result.ToListAsync()).Count > 0) ? await Result.FirstOrDefaultAsync() : CreateAsync();
            return playlist;
        }

        internal async Task UpdateAsync(Playlist Playlist)
        {
            await Collection.ReplaceOneAsync((f) => f.Id.Equals(Playlist.Id), Playlist, new UpdateOptions() { IsUpsert = true });
        }

        internal async Task DeleteAsync(ObjectId Id)
        {
            await Collection.DeleteOneAsync((f) => f.Id.Equals(Id));
        }
    }
}
