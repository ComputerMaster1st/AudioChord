using MongoDB.Driver;
using Shared.Music.Collections.Models;
using System;
using System.Threading.Tasks;

namespace Shared.Music.Collections
{
    internal class SongCollection
    {
        private IMongoCollection<MusicMeta> collection;

        internal SongCollection(IMongoCollection<MusicMeta> collection)
        {
            this.collection = collection;
        }

        public async Task<MusicMeta> GetAsync(Guid Id)
        {
            var result = await collection.FindAsync((f) => f.PrimaryId.Equals(Id));

            if ((await result.ToListAsync()).Count < 1) return null;

            return await result.FirstOrDefaultAsync();
        }
    }
}
