using MongoDB.Driver;

namespace Shared.Music
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
