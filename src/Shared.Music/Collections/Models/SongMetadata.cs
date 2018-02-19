using System;

namespace Shared.Music.Collections.Models
{
    public class SongMetadata
    {
        public string Name { get; private set; }
        public TimeSpan Length { get; private set; }
        public string Uploader { get; private set; }

        internal SongMetadata(string name, TimeSpan length, string uploader)
        {
            Name = name;
            Length = length;
            Uploader = uploader;
        }
    }
}
