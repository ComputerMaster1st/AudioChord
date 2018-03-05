using System;

namespace AudioChord
{
    public class SongMetadata
    {
        public string Name { get; private set; }
        public TimeSpan Length { get; private set; }
        public string Uploader { get; private set; }
        public string Url { get; private set; }

        internal SongMetadata(string name, TimeSpan length, string uploader, string url)
        {
            Name = name;
            Length = length;
            Uploader = uploader;
            Url = url;
        }
    }
}
