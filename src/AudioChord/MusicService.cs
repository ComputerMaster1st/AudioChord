using MongoDB.Bson;
using MongoDB.Driver;
using AudioChord.Collections;
using AudioChord.Collections.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YoutubeExplode;
using AudioChord.Events;
using System.Threading;
using System.Timers;
using System.Collections.Concurrent;

namespace AudioChord
{
    public class MusicService
    {
        private PlaylistCollection playlistCollection;
        private SongCollection songCollection;
        private System.Timers.Timer resyncTimer = new System.Timers.Timer();

        public event EventHandler<ProcessedSongEventArgs> ProcessedSong;
        public event EventHandler<SongsExistedEventArgs> SongsExisted;
        private Task QueueProcessor = null;
        private ConcurrentDictionary<string, ProcessSongRequestInfo> QueuedSongs = new ConcurrentDictionary<string, ProcessSongRequestInfo>();

        private SemaphoreSlim QueueProcessorLock = new SemaphoreSlim(1,1);

        public MusicService(MusicServiceConfig config)
        {
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

            resyncTimer.Interval = TimeSpan.FromHours(24).TotalMilliseconds;
            resyncTimer.AutoReset = true;
            resyncTimer.Elapsed += async (obj, args) => await Resync();
            resyncTimer.Enabled = true;

            resyncTimer.Start();
        }

        /// <summary>
        /// Create a new playlist.
        /// </summary>
        public async Task<Playlist> CreatePlaylist()
        {
            return await playlistCollection.Create();
        }

        /// <summary>
        /// Retrieve your playlist from database.
        /// </summary>
        /// <param name="playlistId">Place playlist Id to fetch.</param>
        /// <returns>A <see cref="Playlist"/> Playlist contains list of all available song Ids.</returns>
        public async Task<Playlist> GetPlaylistAsync(ObjectId playlistId)
        {
            return await playlistCollection.GetPlaylistAsync(playlistId);
        }

        /// <summary>
        /// Delete the playlist from database.
        /// </summary>
        /// <param name="playlistId">The playlist Id to delete.</param>
        public async Task DeletePlaylistAsync(ObjectId playlistId)
        {
            await playlistCollection.DeleteAsync(playlistId);
        }

        /// <summary>
        /// Fetch song metadata with opus stream from database.
        /// </summary>
        /// <param name="songId">The song Id.</param>
        /// <returns>A <see cref="Song"/> SongStream contains song metadata and opus stream.</returns>
        public async Task<Song> GetSongAsync(string songId)
        {
            SongData songData = await songCollection.GetSongAsync(songId);

            return new Song(songData.Id, songData.Metadata, songData.OpusId, songCollection);
        }

        /// <summary>
        /// Get all songs in database.
        /// </summary>
        /// <returns>Dictionary of songs in database.</returns>
        public async Task<IEnumerable<Song>> GetAllSongsAsync()
        {
            List<Song> songList = new List<Song>();
            List<SongData> songDataList =  await songCollection.GetAllAsync();

            if (songDataList.Count > 0)
                foreach (SongData data in songDataList)
                    songList.Add(new Song(data.Id, data.Metadata, data.OpusId, songCollection));

            return songList;
        }

        /// <summary>
        /// Get total bytes count in database.
        /// </summary>
        /// <returns>A double containing total bytes used.</returns>
        public async Task<double> GetTotalBytesUsedAsync()
        {
            return await songCollection.GetTotalBytesUsedAsync();
        }

        // ===============
        // ALL PROCESSOR BASED METHODS GO BELOW THIS COMMENT!
        // ===============

        /// <summary>
        /// Capture Youtube Video Id
        /// </summary>
        /// <param name="url">The youtube video url.</param>
        /// <returns>Return youtube video id.</returns>
        public string ParseYoutubeUrl(string url)
        {
            if (!YoutubeClient.TryParseVideoId(url, out string videoId))
                throw new ArgumentException("Video Url could not be parsed!");
            return videoId;
        }

        /// <summary>
        /// Download song from YouTube to database. (Note: Exceptions are to be expected.)
        /// </summary>
        /// <param name="url">The youtube video url.</param>
        /// <returns>Returns ObjectId of newly downloaded song.</returns>
        public async Task<Song> DownloadSongFromYouTubeAsync(string url)
        {
            string id = await songCollection.DownloadFromYouTubeAsync(url);
            return await GetSongAsync(id);
        }

        /// <summary>
        /// Download song from Discord to database. (Note: Exceptions are to be expected.)
        /// </summary>
        /// <param name="url">The discord attachment url.</param>
        /// <param name="uploader">The discord username.</param>
        /// <param name="attachmentId">The discord attachment Id.</param>
        /// <param name="autoDownload">Automatically download if non-existent.</param>
        /// <returns>Returns ObjectId of newly downloaded song.</returns>
        public async Task<Song> DownloadSongFromDiscordAsync(string url, string uploader, ulong attachmentId)
        {
            string id = await songCollection.DownloadFromDiscordAsync(url, uploader, attachmentId);
            return await GetSongAsync(id);
        }

        // ===============
        // ALL PLAYLIST HANDLING METHODS GO BELOW THIS COMMENT!
        // ===============

        public async Task ProcessYTPlaylistAsync(string youtubePlaylistUrl, ulong guildId, ulong textChannelId, ObjectId playlistId)
        {
            // Get YT playlist from user
            string youtubePlaylistId = YoutubeClient.ParsePlaylistId(youtubePlaylistUrl);
            YoutubeClient youtubeClient = new YoutubeClient();
            YoutubeExplode.Models.Playlist youtubePlaylist = await youtubeClient.GetPlaylistAsync(youtubePlaylistId);

            List<string> existingSongs = new List<string>();
            
            // Halt queue until playlist is processed
            await QueueProcessorLock.WaitAsync();

            // Create & queue up task requests
            foreach (YoutubeExplode.Models.Video video in youtubePlaylist.Videos)
            {
                // Check if song exists
                Song songData = await GetSongAsync($"YOUTUBE#{video.Id}");

                if (songData != null)
                {
                    if (!existingSongs.Contains(songData.Id)) existingSongs.Add(songData.Id);
                    continue;
                }

                // \/ If doesn't exist \/
                ProcessSongRequestInfo info;

                if (!QueuedSongs.TryAdd(video.Id, new ProcessSongRequestInfo($"https://youtu.be/{video.Id}", guildId, textChannelId, playlistId)))
                {
                    info = QueuedSongs[video.Id];
                    if (!info.GuildsRequested.ContainsKey(guildId)) info.GuildsRequested.Add(guildId, new Tuple<ulong, ObjectId>(textChannelId, playlistId));
                }
            }

            // Fire SongsAlreadyExisted Handler
            if (existingSongs.Count > 0) SongsExisted.Invoke(this, new SongsExistedEventArgs(guildId, textChannelId, existingSongs));

            // Start Processing Song Queue
            QueueProcessorLock.Release();
            if (QueueProcessor == null || QueueProcessor.IsCompleted)
            {
                if (QueueProcessor.IsCompleted) QueueProcessor.Dispose();
                QueueProcessor = Task.Run(ProcessRequestedSongsQueueAsync);
            }
        }

        private async Task ProcessRequestedSongsQueueAsync()
        {
            foreach (var infoKeyValue in QueuedSongs)
            {
                // Get the lock
                await QueueProcessorLock.WaitAsync();

                // Process the song
                Song song = await DownloadSongFromYouTubeAsync(infoKeyValue.Value.VideoId);

                // Save to Playlist
                foreach (var guildKeyValue in infoKeyValue.Value.GuildsRequested)
                {
                    Playlist playlist = await GetPlaylistAsync(guildKeyValue.Value.Item2);

                    playlist.Songs.Add(song.Id);
                    await playlist.SaveAsync();

                    // Trigger event upon 1 song completing
                    ProcessedSong.Invoke(this, new ProcessedSongEventArgs(song.Id, song.Metadata.Name, guildKeyValue.Key, guildKeyValue.Value.Item1));
                }
                
                // Release Lock
                QueueProcessorLock.Release();
            }
        }

        // ===============
        // ALL PRIVATE METHODS GO BELOW THIS COMMENT!
        // ===============

        private async Task Resync()
        {
            List<SongData> expiredSongs = new List<SongData>();
            List<SongData> songList = await songCollection.GetAllAsync();

            foreach (SongData song in songList)
                if (song.LastAccessed < DateTime.Now.AddDays(-90))
                    expiredSongs.Add(song);

            if (expiredSongs.Count < 1) return;

            List<Playlist> playlists = await playlistCollection.GetAllAsync();

            foreach (Playlist playlist in playlists)
                foreach (SongData song in expiredSongs)
                    if (playlist.Songs.Contains(song.Id))
                    {
                        playlist.Songs.Remove(song.Id);
                        await playlist.SaveAsync();
                    }

            foreach (SongData song in expiredSongs)
                await songCollection.DeleteSongAsync(song);
        }

        // ===============
        // ALL EVENT HANDLER METHODS GO BELOW THIS COMMENT!
        // ===============


    }
}