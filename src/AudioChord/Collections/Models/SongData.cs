using MongoDB.Bson.Serialization.Attributes;
using System;

namespace AudioChord.Collections.Models
{
    internal class SongData
    {
        [BsonId]
        public SongId Id { get; private set; }
        public DateTime LastAccessed { get; set; } = DateTime.Now;
        public SongMetadata Metadata { get; private set; }

        internal SongData(SongId id, SongMetadata metadata)
        {
            Id = id;
            Metadata = metadata;
        }
    }
}
