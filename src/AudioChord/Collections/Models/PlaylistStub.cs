using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace AudioChord.Collections.Models
{
    public class PlaylistStub
    {
        [BsonId]
        public ObjectId Id { get; private set; } = ObjectId.GenerateNewId();

        [BsonElement]
        internal List<SongId> SongIds { get; set; }

        public PlaylistStub(ObjectId id)
        {
            Id = id;
        }
    }
}
