using AudioChord.Collections;
using AudioChord.Events;
using AudioChord.Wrappers;

using MongoDB.Bson;
using MongoDB.Driver;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AudioChord
{
    public class MusicService
    {
        private PlaylistCollection playlistCollection;
        private SongCollection songCollection;
        //private System.Timers.Timer resyncTimer = new System.Timers.Timer();

        public YoutubeProcessorWrapper Youtube { get; private set; }
        public DiscordProcessorWrapper Discord { get; private set; }


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

            playlistCollection = new PlaylistCollection(database);
            songCollection = new SongCollection(database);

            //processor wrappers
            Youtube = new YoutubeProcessorWrapper(songCollection, new PlaylistProcessor(songCollection, this));
            Discord = new DiscordProcessorWrapper(songCollection);

            //resyncTimer.Interval = TimeSpan.FromHours(12).TotalMilliseconds;
            //resyncTimer.AutoReset = true;
            //resyncTimer.Elapsed += async (obj, args) => await Resync();
            //resyncTimer.Enabled = true;

            //resyncTimer.Start();
        }

        /// <summary>
        /// Create a new playlist.
        /// </summary>
        public Task<Playlist> CreatePlaylist() => playlistCollection.Create();

        /// <summary>
        /// Retrieve your playlist from database.
        /// </summary>
        /// <param name="playlistId">Place playlist Id to fetch.</param>
        /// <returns>A <see cref="Playlist"/> Playlist contains list of all available song Ids.</returns>
        public Task<Playlist> GetPlaylistAsync(ObjectId playlistId) => playlistCollection.GetPlaylistAsync(playlistId);

        /// <summary>
        /// Delete the playlist from database.
        /// </summary>
        /// <param name="playlistId">The playlist Id to delete.</param>
        public Task DeletePlaylistAsync(ObjectId playlistId) => playlistCollection.DeleteAsync(playlistId);

        /// <summary>
        /// Fetch song metadata with opus stream from database.
        /// </summary>
        /// <param name="songId">The song Id.</param>
        /// <returns>A <see cref="Song"/> SongStream contains song metadata and opus stream. Returns null if nothing found.</returns>
        public Task<ISong> GetSongAsync(string songId) => songCollection.GetSongAsync(songId);

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

        // ===============
        // ALL PRIVATE METHODS GO BELOW THIS COMMENT!
        // ===============
        //private async Task Resync()
        //{
        //    await Youtube.QueueProcessorLock.WaitAsync();

        //    List<SongData> expiredSongs = new List<SongData>();
        //    List<SongData> songList = await songCollection.GetAllAsync();
        //    int resyncedPlaylists = 0;
        //    int deletedDesyncedFiles = await songCollection.ResyncDatabaseAsync();
        //    DateTime startedAt = DateTime.Now;

        //    foreach (SongData song in songList)
        //        if (song.LastAccessed < DateTime.Now.AddDays(-90))
        //            expiredSongs.Add(song);

        //    if (expiredSongs.Count > 0)
        //    {
        //        List<Playlist> playlists = await playlistCollection.GetAllAsync();

        //        foreach (Playlist playlist in playlists)
        //        {
        //            int removedSongs = 0;

        //            foreach (SongData song in expiredSongs)
        //                if (playlist.Songs.Contains(song.Id))
        //                {
        //                    removedSongs++;
        //                    playlist.Songs.Remove(song.Id);                            
        //                }

        //            if (removedSongs > 0)
        //            {
        //                await playlistCollection.UpdateAsync(playlist);
        //                resyncedPlaylists++;
        //            }
        //        }

        //        foreach (SongData song in expiredSongs)
        //            await songCollection.DeleteSongAsync(song);
        //    }

        //    Youtube.QueueProcessorLock.Release();

        //    //only invoke the eventhandler if somebody is subscribed to the event
        //    ExecutedResync?.Invoke(this, new ResyncEventArgs(startedAt, deletedDesyncedFiles, expiredSongs.Count, resyncedPlaylists));
        //}

    }
}