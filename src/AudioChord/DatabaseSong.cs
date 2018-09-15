using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AudioChord
{
    public class DatabaseSong : ISong
    {
        [BsonId]
        [BsonSerializer(typeof(SongIdSerializer))]
        public SongId Id { get; private set; }
        public SongMetadata Metadata { get; private set; }

        private readonly Func<DatabaseSong, Task<Stream>> StreamRetriever;

        internal DatabaseSong(SongId id, SongMetadata metadata, Func<DatabaseSong, Task<Stream>> streamRetrieveFunction)
        {
            Id = id;
            Metadata = metadata;
            StreamRetriever = streamRetrieveFunction;
        }

        public Task<Stream> GetMusicStreamAsync()
        {
            //invoke the reference to the private function "OpenOpusStreamAsync" in songCollection
            return StreamRetriever(this);
        }
    }
}
