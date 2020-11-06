using MongoDB.Bson.Serialization.Attributes;
using System;

namespace AudioChord.Collections.Models
{
    /// <summary>
    /// Represents Metadata about a song that could exist in the cache
    /// </summary>
    internal class SongInformation
    {
        [BsonId]
        public SongId Id { get; private set; }

        public DateTime LastAccessed { get; set; } = DateTime.Now;
        public SongMetadata Metadata { get; private set; }

        internal SongInformation(SongId id, SongMetadata metadata)
        {
            Id = id;
            Metadata = metadata;
        }
    }
}