using AudioChord.Caching;
using AudioChord.Extractors;
using System;
using System.Collections.Generic;
using AudioChord.Caching.InMemory;
using AudioChord.Metadata;
using JetBrains.Annotations;

namespace AudioChord
{
    /// <summary>
    /// Configuration object for the music service
    /// </summary>
    [PublicAPI]
    public class MusicServiceConfiguration
    {
        public Func<ISongCache> SongCacheFactory { get; set; } = () => new InMemoryCache();
        public Func<IMetadataProvider> MetadataProviderFactory { get; set; }
        public Func<IReadOnlyCollection<IAudioExtractor>> Extractors { get; set; } = () => new List<IAudioExtractor>();
        public Func<IReadOnlyCollection<IAudioMetadataEnricher>> Enrichers { get; set; } = () => new List<IAudioMetadataEnricher>();
        public ExtractorConfiguration ExtractorConfiguration { get; set; } = new ExtractorConfiguration();

        public string Username { get; set; }
        public string Password { get; set; }
        public string Hostname { get; set; } = "localhost";

        // ReSharper disable once StringLiteralTypo
        public string Database { get; internal set; } = "sharedmusic";
    }
}