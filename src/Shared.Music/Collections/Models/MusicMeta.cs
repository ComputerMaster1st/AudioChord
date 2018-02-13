using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Shared.Music.Collections.Models
{
    public class MusicMeta
    {
        [BsonId] public ObjectId Id { get; private set; } = new ObjectId();
        public string Name { get; private set; }
        public TimeSpan Length { get; private set; }
        public string Uploader { get; private set; }

        internal DateTime LastAccessed { get; set; } = DateTime.Now;
        internal ObjectId OpusId { get; private set; }
    }
}