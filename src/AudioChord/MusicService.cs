using AudioChord.Collections;
using AudioChord.Processors;
using AudioChord.Wrappers;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AudioChord
{
    public class MusicService
    {
        static MusicService()
        {
            BsonSerializer.RegisterSerializer(new SongIdSerializer());
        }

        private PlaylistCollection playlistCollection;
        private SongCollection songCollection;

        public YoutubeProcessorWrapper Youtube { get; private set; }
        public DiscordProcessorWrapper Discord { get; private set; }
        public PlaylistCollection Playlist { get; private set; }

        public MusicService(MusicServiceConfig config)
        {
            // This will tell NETCore 2.1 to use older httpclient. Newer version has SSL issues
            AppContext.SetSwitch("System.Net.Http.UseSocketsHttpHandler", false);

            //Use the builder to allow to connect to database without authentication
            MongoUrlBuilder connectionStringBuilder = new MongoUrlBuilder
            {
                DatabaseName = config.Database,
                Server = new MongoServerAddress(config.Hostname),

                Username = config.Username,
                Password = config.Password
            };

            MongoClient client = new MongoClient(connectionStringBuilder.ToMongoUrl());
            IMongoDatabase database = client.GetDatabase(config.Database);

            playlistCollection = new PlaylistCollection(database, songCollection);
            songCollection = new SongCollection(database);

            //processor wrappers
            Youtube = new YoutubeProcessorWrapper(songCollection, new PlaylistProcessor(songCollection, this));
            Discord = new DiscordProcessorWrapper(songCollection);
        }

        /// <summary>
        /// Fetch song metadata with opus stream from database.
        /// </summary>
        /// <param name="songId">The song Id.</param>
        /// <exception cref="ArgumentException">No song was found with the specified <paramref name="songId"/></exception>
        /// <returns>A <see cref="ISong"/> with metadata and opus stream.</returns>
        public Task<ISong> GetSongAsync(SongId id) => songCollection.GetSongAsync(id);

        /// <summary>
        /// Try to get the song
        /// </summary>
        /// <param name="id">The <see cref="SongId"/> to search for</param>
        /// <param name="value">the action that will (always) be invoked with the return value</param>
        /// <returns></returns>
        public async Task<bool> TryGetSongAsync(SongId id, Action<ISong> value)
            => await songCollection.TryGetSongAsync(id, (actualSong) => { value(actualSong); });

        /// <summary>
        /// Get all songs in database.
        /// </summary>
        /// <returns>Dictionary of songs in database.</returns>
        public Task<IEnumerable<ISong>> GetAllSongsAsync() => songCollection.GetAllAsync();

        /// <summary>
        /// Get total bytes count in database.
        /// </summary>
        /// <returns>A double containing total bytes used.</returns>
        public Task<double> GetTotalBytesUsedAsync() => songCollection.GetTotalBytesUsedAsync();
    }
}