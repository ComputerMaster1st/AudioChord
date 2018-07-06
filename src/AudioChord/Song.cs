using System.IO;
using System.Threading.Tasks;

namespace AudioChord
{
    public class Song : ISong
    {
        public string Id { get; private set; }
        public SongMetadata Metadata { get; private set; }

        private Stream songStream;

        internal Song(string id, SongMetadata metadata, Stream stream)
        {
            Id = id;
            Metadata = metadata;
            songStream = stream;
        }

        public Task<Stream> GetMusicStreamAsync()
        {
            return Task.FromResult(songStream);
        }
    }
}
