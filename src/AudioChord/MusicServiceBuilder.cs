using System;
using System.Collections.Generic;
using AudioChord.Caching;
using AudioChord.Extractors;
using AudioChord.Metadata;
using JetBrains.Annotations;

namespace AudioChord
{
    [PublicAPI]
    public class MusicServiceBuilder
    {
        private MusicServiceConfiguration _configuration = new MusicServiceConfiguration();
        private readonly List<IAudioExtractor> _extractors = new List<IAudioExtractor>();
        

        public MusicServiceBuilder WithMetadataProvider(IMetadataProvider provider)
        {
            _configuration.MetadataProviderFactory = () => provider;
            return this;
        }
        
        public MusicServiceBuilder WithCache(ISongCache cache)
        {
            _configuration.SongCacheFactory = () => cache;
            return this;
        }

        public MusicServiceBuilder WithExtractor<TExtractor>()
            where TExtractor : IAudioExtractor, new()
        {
            return WithExtractor(new TExtractor());
        }
        
        public MusicServiceBuilder WithExtractor(IAudioExtractor extractor)
        {
            _extractors.Add(extractor);
            return this;
        }

        public MusicServiceBuilder Configure(Func<MusicServiceConfiguration, MusicServiceConfiguration> callback)
        {
            _configuration = callback(_configuration);
            return this;
        }

        public MusicServiceConfiguration Build()
        {
            _configuration.Extractors = () => _extractors.AsReadOnly();
            return _configuration;
        }
    }
    
    
}