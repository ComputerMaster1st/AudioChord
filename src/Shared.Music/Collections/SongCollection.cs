using MongoDB.Driver;
using Shared.Music.Collections.Models;

namespace Shared.Music.Collections
{
    internal class SongCollection
    {
        private IMongoCollection<Song> collection;

        internal SongCollection(IMongoCollection<Song> collection)
        {
            this.collection = collection;
        }
    }
}
