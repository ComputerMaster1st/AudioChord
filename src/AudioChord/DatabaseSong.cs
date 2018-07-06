using System;
using System.IO;
using System.Threading.Tasks;

namespace AudioChord
{
    public class DatabaseSong : ISong
    {
        public string Id { get; private set; }
        public SongMetadata Metadata { get; private set; }

        private readonly Func<DatabaseSong, System.Threading.Tasks.Task<Stream>> StreamRetriever;

        internal DatabaseSong(string id, SongMetadata metadata, Func<DatabaseSong, System.Threading.Tasks.Task<Stream>> streamRetrieveFunction)
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
