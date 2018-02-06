using MongoDB.Driver;
using Shared.Music.Collections.Models;
using System;
using System.Threading.Tasks;

namespace Shared.Music.Collections
{
    internal class SongCollection
    {
        private IMongoCollection<Song> collection;

        internal SongCollection(IMongoCollection<Song> collection)
        {
            this.collection = collection;
        }

        public async Task<Song> GetAsync(Guid Id)
        {
            var result = await collection.FindAsync((f) => f.Id.Equals(Id));
            return await result.FirstOrDefaultAsync();
        }
    }
}
