using MongoDB.Driver;
using Shared.Music.Collections.Models;

namespace Shared.Music.Collections
{
    internal class PlaylistCollection
    {
        private IMongoCollection<PlaylistMeta> collection;

        internal PlaylistCollection(IMongoCollection<PlaylistMeta> collection)
        {
            this.collection = collection;
        }
    }
}
