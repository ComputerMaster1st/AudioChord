using System.IO;
using System.Threading.Tasks;

namespace AudioChord
{
    public interface ISong
    {
        SongId Id { get; }
        SongMetadata Metadata { get; }
        Task<Stream> GetMusicStreamAsync();
    }
}