using MongoDB.Bson;
using System.IO;

namespace Shared.Music.Collections.Models
{
    public class Song
    {
        public ObjectId Id { get; private set; }
        public SongMetadata Metadata { get; private set; }
        public Stream Music { get; private set; }

        public Song(SongMetadata metadata, Stream song, ObjectId internalId)
        {
            Id = internalId;
            Metadata = metadata;
            Music = song;
        }
    }
}