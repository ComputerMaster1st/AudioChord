using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Shared.Music.Collections.Models
{
    public class Song
    {
        [BsonId] public ObjectId Id { get; private set; } = ObjectId.GenerateNewId();
        public string Name { get; private set; }
        public TimeSpan Length { get; private set; }
        public string Uploader { get; private set; }

        [BsonElement] internal DateTime LastAccessed { get; set; } = DateTime.Now;
        [BsonElement] internal ObjectId OpusId { get; private set; }

        public Song(string Name, TimeSpan Length, string Uploader, ObjectId OpusId)
        {
            this.Name = Name;
            this.Length = Length;
            this.Uploader = Uploader;
            this.OpusId = OpusId;
        }
    }
}