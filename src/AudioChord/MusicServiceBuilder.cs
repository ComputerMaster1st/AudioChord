using System;
using AudioChord.Caching;
using AudioChord.Metadata;
using JetBrains.Annotations;

namespace AudioChord
{
    [PublicAPI]
    public class MusicServiceBuilder
    {
        private MusicServiceConfiguration _configuration = new MusicServiceConfiguration();

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

        public MusicServiceBuilder Configure(Func<MusicServiceConfiguration, MusicServiceConfiguration> callback)
        {
            _configuration = callback(_configuration);
            return this;
        }

        public MusicServiceConfiguration Build()
        {
            return _configuration;
        }
    }
    
    
}