using MongoDB.Driver;
using Shared.Music.Collections;
using Shared.Music.Collections.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shared.Music
{
    public class MusicService
    {
        private PlaylistCollection Playlists;
        private SongCollection Songs;

        public MusicService(MusicServiceConfig config)
        {
            MongoClient client = new MongoClient($"mongodb://{config.Username}:{config.Password}@localhost:27017/sharedmusic");
            IMongoDatabase database = client.GetDatabase("sharedmusic");

            Playlists = new PlaylistCollection(database.GetCollection<Playlist>(typeof(Playlist).Name));
            Songs = new SongCollection(database);
        }

        public async Task<Guid> CreatePlaylistAsync()
        {
            return await Playlists.CreateAsync();
        }

        public async Task<List<MusicMeta>> GetPlaylistAsync(Guid Id)
        {
            Playlist playlist = await Playlists.GetAsync(Id);
            List<MusicMeta> Playlist = new List<MusicMeta>();
            
            foreach (Guid SongId in playlist.SongList)
            {
                Playlist.Add(await GetSongAsync(SongId));
            }

            return Playlist;
        }

        public async Task DeletePlaylistAsync(Guid Id)
        {
            await Playlists.DeleteAsync(Id);
        }

        public async Task AddSongToPlaylistAsync()
        {
            /// TODO: Add Song To Playlist
            /// TODO: Process the newly added song using YoutubeExplode, FFMPEG, etc
            throw new NotImplementedException("Post Song To Playlist Not Yet Implemented");
        }

        public async Task<bool> DeleteSongFromPlaylistAsync(Guid PlaylistId, Guid SongId)
        {
            Playlist playlist = await Playlists.GetAsync(PlaylistId);

            if (playlist.SongList.Contains(SongId)) return false;

            playlist.SongList.Add(SongId);
            await Playlists.UpdateAsync(PlaylistId, playlist);
            return true;
        }

        public async Task<MusicMeta> GetSongAsync(Guid Id)
        {
            return await Songs.GetAsync(Id);
        }

        public async Task<MusicStream> GetOpusStreamAsync(MusicMeta Song)
        {
            return await Songs.GetStreamAsync(Song);
        }

        private async Task ResyncAsync()
        {
            /// TODO: Delete unused songs. Automate if possible.
        }
    }
}