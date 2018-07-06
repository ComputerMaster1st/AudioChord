using System.IO;
using System.Threading.Tasks;

namespace AudioChord
{
    public interface ISong
    {
        string Id { get; }
        SongMetadata Metadata { get; }
        Task<Stream> GetMusicStreamAsync();
    }
}