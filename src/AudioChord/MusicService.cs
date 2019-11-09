using AudioChord.Collections;
using AudioChord.Processors;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using AudioChord.Exceptions;
using AudioChord.Facades;

namespace AudioChord
{
    public class MusicService
    {
        static MusicService()
        {
            BsonSerializer.RegisterSerializer(new SongIdSerializer());
        }

        private readonly SongCollection _songCollection;

        public YoutubeProcessorFacade Youtube { get; }
        public DiscordProcessorFacade Discord { get; }
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

            _songCollection = new SongCollection(database, config.SongCacheFactory());

            Playlist = new PlaylistCollection(database, _songCollection);

            // Processor facades
            Youtube = new YoutubeProcessorFacade(
                _songCollection, 
                new PlaylistProcessor(
                    _songCollection, 
                    config.ExtractorConfiguration, 
                    this
                ),
                config.ExtractorConfiguration
            );
            
            Discord = new DiscordProcessorFacade(_songCollection);
        }

        /// <summary>
        /// Fetch song metadata with opus stream from database.
        /// </summary>
        /// <param name="id">The song Id.</param>
        /// <exception cref="SongNotFoundException">No song was found with the specified <paramref name="id"/></exception>
        /// <returns>A <see cref="ISong"/> with metadata and opus stream.</returns>
        public async Task<ISong> GetSongAsync(SongId id)
        {
            (bool success, ISong result) = await _songCollection.TryGetSongAsync(id);

            if (!success)
                throw new SongNotFoundException($"The song-id '{id}' was not found!");

            return result;
        }

        /// <summary>
        /// Try to get the song
        /// </summary>
        /// <param name="id">The <see cref="SongId"/> to search for</param>
        /// <returns></returns>
        public Task<(bool, ISong)> TryGetSongAsync(SongId id)
            => _songCollection.TryGetSongAsync(id);

        /// <summary>
        /// Return a random list of songs
        /// </summary>
        /// <param name="amount">The amount of random songs, defaults to 100</param>
        /// <returns>A list of SongId's</returns>
        public IEnumerable<SongId> GetRandomSongs(long amount = 100)
            => _songCollection.GetRandomSongs(amount);

        /// <summary>
        /// Get all songs in database.
        /// </summary>
        /// <returns>Dictionary of songs in database.</returns>
        public Task<IEnumerable<SongMetadata>> EnumerateSongMetadataAsync()
            => _songCollection.EnumerateMetadataAsync();

        public bool TryGetSongMetadata(SongId id, out SongMetadata metadata)
        {
            metadata = _songCollection.TryFindSongMetadata(id);
            return !(metadata is null);
        }

        /// <summary>
        /// Get total bytes count in database.
        /// </summary>
        /// <returns>A double containing total bytes used.</returns>
        public Task<double> GetTotalBytesUsedAsync()
            => Task.FromResult(0d);
    }
}