using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Shared.Music.Collections.Models
{
    public class Song
    {
        [BsonId] public Guid Id { get; private set; } = new Guid();
        public string Name { get; private set; }
        public TimeSpan Length { get; private set; }
        public string Uploader { get; private set; }
        internal ObjectId OpusId { get; private set; }
    }
}