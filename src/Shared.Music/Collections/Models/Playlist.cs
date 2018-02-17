using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shared.Music.Collections.Models
{
    public class Playlist
    {
        [BsonId] internal ObjectId Id { get; private set; } = ObjectId.GenerateNewId();
        public List<ObjectId> Songs { get; private set; } = new List<ObjectId>();

        [BsonIgnore] private PlaylistCollection collection;

        internal Playlist(PlaylistCollection collection)
        {
            this.collection = collection;
        }

        public async Task SaveAsync()
        {
            await collection.UpdateAsync(this);
        }
    }
}