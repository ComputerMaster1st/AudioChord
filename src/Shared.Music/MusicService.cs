using MongoDB.Driver;
using Shared.Music.Collections;
using Shared.Music.Collections.Models;
using System;
using System.Threading.Tasks;

namespace Shared.Music
{
    public class MusicService
    {
        private PlaylistCollection Playlists;

        public MusicService(MusicServiceConfig config)
        {
            MongoClient client = new MongoClient($"mongodb://{config.Username}:{config.Password}@localhost:27017/sharedmusic");
            IMongoDatabase database = client.GetDatabase("SharedMusic");

            Playlists = new PlaylistCollection(database.GetCollection<Playlist>(typeof(Playlist).Name));
        }

        public async Task<Guid> CreatePlaylistAsync()
        {
            /// Create Empty Playlist
            return await Playlists.CreateAsync();
        }

        public async Task<Playlist> GetPlaylistAsync(Guid Id)
        {
            /// TODO: Get Specified Playlist
            return await Playlists.GetAsync(Id);
        }

        public async Task DeletePlaylistAsync(Guid Id)
        {
            /// Delete Specified Playlist
            await Playlists.DeleteAsync(Id);
        }

        public async Task AddSongToPlaylistAsync()
        {
            /// TODO: Add Song To Playlist
            /// TODO: Process the newly added song using YoutubeExplode, FFMPEG, etc
            throw new NotImplementedException("Post Song To Playlist Not Yet Implemented");
        }

        public async Task DeleteSongFromPlaylistAsync()
        {
            /// TODO: Remove specified song from playlist
        }

        public async Task GetSongAsync()
        {
            /// TODO: Get Song Meta Data
        }

        public async Task GetOpusStreamAsync()
        {
            /// TODO: Create an Opus Stream for specified song
        }

        private async Task ResyncAsync()
        {
            /// TODO: Delete unused songs. Automate if possible.
        }
    }
}