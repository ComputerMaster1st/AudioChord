using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace Shared.Music.Collections.Models
{
    public class Playlist
    {
        [BsonId] public ObjectId Id { get; internal set; } = new ObjectId();
        public List<ObjectId> SongList { get; internal set; } = new List<ObjectId>();
    }
}