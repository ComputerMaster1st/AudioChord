using MongoDB.Bson;
using System.IO;

namespace Shared.Music.Collections.Models
{
    public class SongStream
    {
        public string Id { get; internal set; }
        public SongMeta Metadata { get; internal set; }
        public Stream MusicStream { get; internal set; }
    }
}
