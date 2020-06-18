using System;
using AudioChord.Collections;
using AudioChord.Processors;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioChord.Exceptions;
using AudioChord.Extractors;
using AudioChord.Facades;
using AudioChord.Metadata;
using JetBrains.Annotations;

namespace AudioChord
{
    [PublicAPI]
    public class MusicService
    {
        static MusicService()
        {
            BsonSerializer.RegisterSerializer(new SongIdSerializer());
        }

        private readonly SongCollection _songCollection;
        private readonly IReadOnlyCollection<IAudioExtractor> _extractors;
        private readonly IReadOnlyCollection<IAudioMetadataEnricher> _enrichers;
        private readonly ExtractorConfiguration _extractorConfiguration;
        private readonly IMetadataProvider _provider;

        [Obsolete("Use MusicService.DownloadAsync with an url instead")]
        public YoutubeProcessorFacade Youtube { get; }
        
        [Obsolete("Playlist support will be removed in a future release")]
        public PlaylistCollection Playlist { get; }

        public MusicService(MusicServiceConfiguration config)
        {
            // Use the builder to allow to connect to database without authentication
            MongoUrlBuilder connectionStringBuilder = new MongoUrlBuilder
            {
                DatabaseName = config.Database,
                Server = new MongoServerAddress(config.Hostname),

                Username = config.Username,
                Password = config.Password
            };

            MongoClient client = new MongoClient(connectionStringBuilder.ToMongoUrl());
            IMongoDatabase database = client.GetDatabase(config.Database);
            _provider = config.MetadataProviderFactory();

            _songCollection = new SongCollection(_provider, config.SongCacheFactory());

            Playlist = new PlaylistCollection(database, _songCollection);

            // Processor facades
            Youtube = new YoutubeProcessorFacade(
                _songCollection, 
                new PlaylistProcessor(
                    _songCollection,
                    this
                ),
                config.ExtractorConfiguration
            );

            _extractors = config.Extractors();
            _enrichers = config.Enrichers();
            _extractorConfiguration = config.ExtractorConfiguration;
        }

        /// <summary>
        /// Fetch song metadata with opus stream from database.
        /// </summary>
        /// <param name="id">The song Id.</param>
        /// <exception cref="SongNotFoundException">No song was found with the specified <paramref name="id"/></exception>
        /// <returns>A <see cref="ISong"/> with metadata and opus stream.</returns>
        public async Task<ISong?> GetSongAsync(SongId id) 
            => await _songCollection.TryGetSongAsync(id);

        /// <summary>
        /// Try to download the song at given url, if the song is cached and the cache is enabled
        /// then the song in the cache is returned
        /// </summary>
        /// <param name="url">The source to download from</param>
        /// <param name="ignoreCache">Download the audio directly from the source, do not store the audio in the cache</param>
        /// <returns>An <see cref="ISong"/> with the song at the given source</returns>
        /// <exception cref="NoExtractorCandidatesException">No Extractors found that an handle the given url</exception>
        /// <exception cref="InvalidOperationException">All possible extractors failed to extract audio</exception>
        /// <exception cref="FormatException">Failed to extract an id using the selected extractor</exception>
        public async Task<ISong> DownloadSongAsync(string url, bool ignoreCache = false)
        {
            IList<IAudioExtractor> extractorCandidates = _extractors
                .Where(extract => extract.CanExtract(url))
                .ToList();

            if (extractorCandidates.Count == 0)
                throw new NoExtractorCandidatesException($"No extractors found for url '{url}'");

            foreach (IAudioExtractor extractorCandidate in extractorCandidates)
            {
                if (!ignoreCache)
                {
                    // Attempt to fetch an id from the url and check the cache
                    if (!extractorCandidate.TryExtractSongId(url, out SongId id))
                        throw new FormatException($"Could not extract an id from '{url}'!");
                    
                    ISong? song = await GetSongAsync(id);
                    if (song != null)
                        return song;
                }

                // Failed.. Try to extract it
                ISong extractedSong = await extractorCandidate.ExtractAsync(url, _extractorConfiguration);
            
                // Run enrichers over the song to allow them to update the metadata if needed
                foreach (IAudioMetadataEnricher enricher in _enrichers) 
                    await enricher.EnrichAsync(extractedSong);

                // And finally cache it
                return await _songCollection.StoreSongAsync(extractedSong);
            }

            throw new InvalidOperationException($"All {extractorCandidates.Count} Audio Extractors failed to extract audio");
        }

        /// <summary>
        /// Return a random list of songs
        /// </summary>
        /// <param name="amount">The amount of random songs, defaults to 100</param>
        /// <returns>A list of SongId's</returns>
        public Task<IEnumerable<SongId>> GetRandomSongs(long amount = 100)
            => _songCollection.GetRandomSongs(amount);

        /// <summary>
        /// Get all audio metadata in database.
        /// </summary>
        /// <returns>Enumerable of Audio Metadata in the shape of <see cref="SongMetadata"/>.</returns>
        public IAsyncEnumerable<SongMetadata> EnumerateSongMetadataAsync()
            => _provider.GetAllMetadataAsync();

        /// <summary>
        /// Find a single audio metadata entry
        /// </summary>
        /// <param name="id">The <see cref="SongId"/> to look for</param>
        /// <returns>null if the song metadata does not exist</returns>
        public Task<SongMetadata?> TryGetSongMetadata(SongId id) 
            => _provider.GetSongMetadataAsync(id);

        /// <summary>
        /// Get total bytes count in database.
        /// </summary>
        /// <returns>A double containing total bytes used.</returns>
        [Obsolete("Will be replaced with a new reporting system, does nothing")]
        public Task<double> GetTotalBytesUsedAsync()
            => Task.FromResult(0d);
    }
}