using System.IO;

namespace Shared.Music.Collections.Models
{
    public class MusicStream : MusicMeta
    {
        public Stream OpusStream { get; internal set; }
    }
}
