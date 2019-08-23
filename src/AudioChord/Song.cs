using System.IO;
using System.Threading.Tasks;

namespace AudioChord
{
    public class Song : ISong
    {
        public SongId Id { get; private set; }
        public SongMetadata Metadata { get; private set; }

        private readonly Stream _songStream;

        internal Song(SongId id, SongMetadata metadata, Stream stream)
        {
            Id = id;
            Metadata = metadata;
            _songStream = stream;
        }

        public Task<Stream> GetMusicStreamAsync()
            => Task.FromResult(_songStream);
    }
}