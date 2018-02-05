using MongoDB.Driver;
using Shared.Music.Collections.Models;

namespace Shared.Music.Collections
{
    internal class PlaylistCollection
    {
        private IMongoCollection<Playlist> collection;

        internal PlaylistCollection(IMongoCollection<Playlist> collection)
        {
            this.collection = collection;
        }
    }
}
