using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Shared.Music
{
    internal class SongMeta
    {
        [BsonId] public Guid Id { get; private set; } = new Guid();
        public string Name { get; private set; }
        public TimeSpan Length { get; private set; }
        public string Uploader { get; private set; }
    }
}