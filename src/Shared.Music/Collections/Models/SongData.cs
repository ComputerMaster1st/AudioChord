using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Shared.Music.Collections.Models
{
    internal class SongData
    {
        [BsonId] public ObjectId Id { get; private set; }
        public DateTime LastAccessed { get; set; } = DateTime.Now;
        public ObjectId OpusId { get; private set; }
        public SongMeta Metadata { get; private set; }

        internal SongData(ObjectId songId, ObjectId opusId, SongMeta songMeta)
        {
            Id = songId;
            OpusId = opusId;
            Metadata = songMeta;
        }
    }
}
