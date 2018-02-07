using System.IO;

namespace Shared.Music.Collections.Models
{
    public class MusicStream : Music
    {
        public Stream OpusStream { get; internal set; }
    }
}
