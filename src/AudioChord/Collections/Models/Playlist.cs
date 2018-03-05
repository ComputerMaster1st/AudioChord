using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AudioChord.Collections.Models
{
    public class Playlist
    {
        [BsonId] public ObjectId Id { get; private set; } = ObjectId.GenerateNewId();
        public List<string> Songs { get; private set; } = new List<string>();

        [BsonIgnore] internal PlaylistCollection collection;

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