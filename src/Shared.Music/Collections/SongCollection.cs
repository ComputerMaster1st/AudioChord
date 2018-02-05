using MongoDB.Driver;
using Shared.Music.Collections.Models;

namespace Shared.Music.Collections
{
    internal class SongCollection
    {
        private IMongoCollection<SongMeta> collection;

        internal SongCollection(IMongoCollection<SongMeta> collection)
        {
            this.collection = collection;
        }
    }
}
