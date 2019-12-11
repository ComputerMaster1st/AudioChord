using AudioChord.Caching;
using AudioChord.Extractors;
using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace AudioChord
{
    /// <summary>
    /// Configuration object for the music service
    /// </summary>
    [PublicAPI]
    public class MusicServiceConfiguration
    {
        public Func<ISongCache> SongCacheFactory { get; set; }
        public Func<IReadOnlyCollection<IAudioExtractor>> Extractors { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }
        public string Hostname { get; set; } = "localhost";

        // ReSharper disable once StringLiteralTypo
        public string Database { get; internal set; } = "sharedmusic";
        public ExtractorConfiguration ExtractorConfiguration { get; set; } = new ExtractorConfiguration();
    }
}