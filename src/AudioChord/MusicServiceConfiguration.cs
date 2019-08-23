using AudioChord.Caching;
using System;

namespace AudioChord
{
    public class MusicServiceConfiguration
    {
        public Func<ISongCache> SongCacheFactory { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }
        public string Hostname { get; set; } = "localhost";

        // ReSharper disable once StringLiteralTypo
        public string Database { get; internal set; } = "sharedmusic";
    }
}