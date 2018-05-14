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
using System.Collections.Concurrent;

namespace AudioChord
{
    public class MusicService
    {
        private PlaylistCollection playlistCollection;
        private SongCollection songCollection;
        private System.Timers.Timer resyncTimer = new System.Timers.Timer();

        public event EventHandler<ResyncEventArgs> ExecutedResync;
        public event EventHandler<ProcessedSongEventArgs> ProcessedSong;
        public event EventHandler<SongsExistedEventArgs> SongsExisted;

        private Task QueueProcessor = null;
        private ConcurrentDictionary<string, ProcessSongRequestInfo> QueuedSongs = new ConcurrentDictionary<string, ProcessSongRequestInfo>();
        private SemaphoreSlim QueueProcessorLock = new SemaphoreSlim(1, 1);
        private Dictionary<ulong, int> QueueGuildStatus = new Dictionary<ulong, int>();

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

            resyncTimer.Interval = TimeSpan.FromHours(12).TotalMilliseconds;
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
        /// <returns>A <see cref="Song"/> SongStream contains song metadata and opus stream. Returns null if nothing found.</returns>
        public async Task<Song> GetSongAsync(string songId)
        {
            SongData songData = await songCollection.GetSongAsync(songId);

            if (songData != null) return new Song(songData.Id, songData.Metadata, songData.OpusId, songCollection);
            else return null;
        }

        /// <summary>
        /// Get all songs in database.
        /// </summary>
        /// <returns>Dictionary of songs in database.</returns>
        public async Task<IEnumerable<Song>> GetAllSongsAsync()
        {
            List<Song> songList = new List<Song>();
            List<SongData> songDataList = await songCollection.GetAllAsync();

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

        /// <summary>
        /// Show how many songs are in the process queue.
        /// </summary>
        public int CurrentQueueLength {
            get {
                return QueuedSongs.Count;
            }
        }

        /// <summary>
        /// Show how many songs from the guild are in the process queue.
        /// </summary>
        public int CurrentGuildQueueLength(ulong guildId)
        {
            return (QueueGuildStatus.ContainsKey(guildId)) ? QueueGuildStatus[guildId] : 0;
        }

        /// <summary>
        /// Download a list of YT songs to database.
        /// </summary>
        public async Task ProcessYTPlaylistAsync(List<string> youtubeUrls, ulong guildId, ulong textChannelId, Playlist playlist)
        {
            // Existing & Queued Counters for Guild's Request
            int installedSongs = 0;
            int existingSongs = 0;
            int queuedSongs = 0;
            int failedParsingSongs = 0;

            // Halt queue until playlist is processed
            await QueueProcessorLock.WaitAsync();

            // Create & queue up queue requests
            foreach (string url in youtubeUrls)
            {
                if (!YoutubeClient.TryParseVideoId(url, out string videoId))
                {
                    failedParsingSongs++;
                    continue;
                }

                // Check if song exists
                videoId = $"YOUTUBE#{videoId}";
                Song songData = await GetSongAsync(videoId);

                if (songData != null)
                {
                    // Add song to playlist if not already
                    if (!playlist.Songs.Contains(songData.Id))
                    {
                        playlist.Songs.Add(songData.Id);
                        installedSongs++;
                    }
                    else existingSongs++;

                    continue;
                }

                // \/ If doesn't exist \/
                ProcessSongRequestInfo info = null;

                if (!QueuedSongs.TryAdd(videoId, new ProcessSongRequestInfo(url, guildId, textChannelId, playlist)))
                {
                    info = QueuedSongs[videoId];

                    if (!info.GuildsRequested.ContainsKey(guildId))
                    {
                        info.GuildsRequested.TryAdd(guildId, new Tuple<ulong, Playlist>(textChannelId, playlist));
                        queuedSongs++;
                    }
                }
                else queuedSongs++;
            }

            if (installedSongs > 0) await playlist.SaveAsync();

            // Fire SongsAlreadyExisted Handler
            SongsExisted.Invoke(this, new SongsExistedEventArgs(guildId, textChannelId, existingSongs, installedSongs, queuedSongs, failedParsingSongs));

            // Start Processing Song Queue
            if (QueuedSongs.Count > 0)
            {
                if (QueueProcessor == null)
                    QueueProcessor = Task.Run(ProcessRequestedSongsQueueAsync);
                else if (QueueProcessor != null && QueueProcessor.Status != TaskStatus.WaitingForActivation)
                {
                    QueueProcessor.Dispose();
                    QueueProcessor = Task.Run(ProcessRequestedSongsQueueAsync);
                }

                // Add/Update The Guild's Music Processing Queue Status
                if (!QueueGuildStatus.TryAdd(guildId, queuedSongs))
                    QueueGuildStatus[guildId] = (QueueGuildStatus[guildId] + queuedSongs);
            }

            QueueProcessorLock.Release();
        }

        /// <summary>
        /// Download a playlist of YT songs to database.
        /// </summary>
        public async Task<bool> ProcessYTPlaylistAsync(string youtubePlaylistUrl, ulong guildId, ulong textChannelId, Playlist playlist)
        {
            // Get YT playlist from user
            if (!YoutubeClient.TryParsePlaylistId(youtubePlaylistUrl, out string youtubePlaylistId)) return false;

            YoutubeClient youtubeClient = new YoutubeClient();
            YoutubeExplode.Models.Playlist youtubePlaylist = await youtubeClient.GetPlaylistAsync(youtubePlaylistId);
            List<string> youtubeUrls = new List<string>();

            // Create a list of all urls to process
            foreach (YoutubeExplode.Models.Video video in youtubePlaylist.Videos) youtubeUrls.Add($"https://youtu.be/{video.Id}");

            // Begin playlist processing
            await ProcessYTPlaylistAsync(youtubeUrls, guildId, textChannelId, playlist);
            return true;
        }

        private async Task ProcessRequestedSongsQueueAsync()
        {
            foreach (var infoKeyValue in QueuedSongs)
            {
                // Get the lock
                await QueueProcessorLock.WaitAsync();

                Song song = null;
                string requestId = infoKeyValue.Key;
                QueuedSongs.TryRemove(infoKeyValue.Key, out ProcessSongRequestInfo info);

                try
                {
                    // Process the song
                    song = await DownloadSongFromYouTubeAsync(info.VideoId);

                    // Save to Playlist
                    foreach (var guildKeyValue in info.GuildsRequested)
                    {
                        try
                        {
                            // Update The Guild's Music Processing Queue Status
                            QueueGuildStatus[guildKeyValue.Key]--;

                            if (song == null)
                            {
                                // Trigger event upon 1 song completing
                                ProcessedSong.Invoke(this, new ProcessedSongEventArgs(requestId, null, guildKeyValue.Key, guildKeyValue.Value.Item1, QueueGuildStatus[guildKeyValue.Key], QueuedSongs.Count));
                            }
                            else
                            {
                                Playlist playlist = guildKeyValue.Value.Item2;
                                playlist.Songs.Add(song.Id);
                                await playlist.SaveAsync();

                                // Trigger event upon 1 song completing
                                ProcessedSong.Invoke(this, new ProcessedSongEventArgs(song.Id, song.Metadata.Name, guildKeyValue.Key, guildKeyValue.Value.Item1, QueueGuildStatus[guildKeyValue.Key], QueuedSongs.Count));
                            }

                            // Remove QueueGuildStatus if completed
                            if (QueueGuildStatus[guildKeyValue.Key] == 0) QueueGuildStatus.Remove(guildKeyValue.Key);
                        }
                        catch { }
                    }
                }
                catch { }
                finally
                {
                    // Release Lock
                    QueueProcessorLock.Release();
                }
            }
        }

        // ===============
        // ALL PRIVATE METHODS GO BELOW THIS COMMENT!
        // ===============

        private async Task Resync()
        {
            await QueueProcessorLock.WaitAsync();

            List<SongData> expiredSongs = new List<SongData>();
            List<SongData> songList = await songCollection.GetAllAsync();

            int deletedDesyncedFiles = await songCollection.ResyncDatabaseAsync();

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

            QueueProcessorLock.Release();

            ExecutedResync.Invoke(this, new ResyncEventArgs(deletedDesyncedFiles, expiredSongs.Count));
        }
    }
}