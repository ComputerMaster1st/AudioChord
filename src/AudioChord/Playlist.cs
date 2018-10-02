using AudioChord.Collections;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AudioChord
{
    public class Playlist
    {
        [BsonId]
        public ObjectId Id { get; private set; } = ObjectId.GenerateNewId();

        public List<ISong> Songs { get; private set; } = new List<ISong>();

        [BsonIgnore]
        internal PlaylistCollection collection;

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