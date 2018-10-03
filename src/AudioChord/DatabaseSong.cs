using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AudioChord
{
    /// <summary>
    /// Song that has been stored in the database
    /// </summary>
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
            // Invoke the reference to the private function "OpenOpusStreamAsync" in songCollection
            => StreamRetriever(this);
        
    }
}
