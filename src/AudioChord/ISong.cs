using System.IO;
using System.Threading.Tasks;

namespace AudioChord
{
    public interface ISong
    {
        SongMetadata Metadata { get; }
        Task<Stream> GetMusicStreamAsync();
    }
}