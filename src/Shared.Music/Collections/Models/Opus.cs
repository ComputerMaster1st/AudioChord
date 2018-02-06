using System.IO;

namespace Shared.Music.Collections.Models
{
    public class Opus : Song
    {
        public Stream OpusStream { get; internal set; }
    }
}
