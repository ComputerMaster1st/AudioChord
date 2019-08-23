using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace AudioChord
{
    public class Playlist
    {
        [BsonId]
        public ObjectId Id { get; private set; }

        public List<SongId> Songs { get; private set; }

        internal Playlist(ObjectId id, List<SongId> songs)
        {
            Id = id;
            Songs = songs;
        }

        public Playlist() : this(ObjectId.GenerateNewId(), new List<SongId>())
        { }
    }
}