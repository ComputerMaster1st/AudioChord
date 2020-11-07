using System.IO;
using System.Threading.Tasks;

namespace AudioChord
{
    public class Song : ISong
    {
        public SongMetadata Metadata { get; private set; }

        private readonly Stream _songStream;

        public Song(SongId id, SongMetadata metadata, Stream stream)
        {
            Metadata = metadata;
            Metadata.Id = id;
            _songStream = stream;
        }

        public Task<Stream> GetMusicStreamAsync()
            => Task.FromResult(_songStream);
    }
}