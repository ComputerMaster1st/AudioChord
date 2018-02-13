using System.IO;

namespace Shared.Music.Collections.Models
{
    public class MusicStream : Song
    {
        public Stream OpusStream { get; internal set; }
    }
}
