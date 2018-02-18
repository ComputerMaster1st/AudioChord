using MongoDB.Bson;
using MongoDB.Driver;
using Shared.Music.Collections;
using Shared.Music.Collections.Models;
using System;
using System.Threading.Tasks;

namespace Shared.Music
{
    public class MusicService
    {
        private PlaylistCollection playlistCollection;
        private SongCollection songCollection;

        public MusicService(MusicServiceConfig config)
        {
            MongoClient client = new MongoClient($"mongodb://{config.Username}:{config.Password}@{config.Hostname}:27017/sharedmusic");
            IMongoDatabase database = client.GetDatabase("sharedmusic");

            playlistCollection = new PlaylistCollection(database);
            songCollection = new SongCollection(database);
        }

        /// <summary>
        /// Create a new playlist.
        /// </summary>
        public Playlist CreatePlaylist()
        {
            return playlistCollection.Create();
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
        /// Fetch song metadata from database.
        /// </summary>
        /// <param name="songId">The song Id.</param>
        /// <returns>A <see cref="SongMeta"/> SongMeta contains song metadata.</returns>
        public async Task<SongMeta> GetSongMetadataAsync(ObjectId songId)
        {
            SongData songData = await songCollection.GetSongAsync(songId);
            return songData.Metadata;
        }

        /// <summary>
        /// Fetch song metadata with opus stream from database.
        /// </summary>
        /// <param name="songId">The song Id.</param>
        /// <returns>A <see cref="SongStream"/> SongStream contains song metadata and opus stream.</returns>
        public async Task<SongStream> GetSongStreamAsync(ObjectId songId)
        {
            SongData songData = await songCollection.GetSongAsync(songId);

            SongStream songStream = new SongStream();
            songStream.Id = songData.Id;
            songStream.Metadata = songData.Metadata;
            songStream.MusicStream = await songCollection.OpenOpusStreamAsync(songData.OpusId);

            songData.LastAccessed = DateTime.Now;
            await songCollection.UpdateSongAsync(songData);

            return songStream;
        }

        // ===============
        // ALL PROCESSOR BASED METHODS GO BELOW THIS COMMENT!
        // ===============

        /// <summary>
        /// Download song from YouTube to database. (Note: Exceptions are to be expected.)
        /// </summary>
        /// <param name="url">The youtube video url.</param>
        /// <returns>Returns ObjectId of newly downloaded song.</returns>
        public async Task<ObjectId> DownloadSongFromYouTubeAsync(string url)
        {
            return await songCollection.DownloadFromYouTubeAsync(url);
        }

        /// <summary>
        /// Download song from Discord to database. (Note: Exceptions are to be expected.)
        /// </summary>
        /// <param name="url">The discord attachment url.</param>
        /// <param name="uploader">The discord username.</param>
        /// <param name="attachmentId">The discord attachment Id.</param>
        /// <returns>Returns ObjectId of newly downloaded song.</returns>
        public async Task<ObjectId> DownloadSongFromDiscordAsync(string url, string uploader, ulong attachmentId)
        {
            return await songCollection.DownloadFromDiscordAsync(url, uploader, attachmentId);
        }

        // ===============
        // ALL PRIVATE METHODS GO BELOW THIS COMMENT!
        // ===============

        private void Resync()
        {
            throw new NotImplementedException("Music Auto-Resync has yet to be implemented!");
        }
    }
}