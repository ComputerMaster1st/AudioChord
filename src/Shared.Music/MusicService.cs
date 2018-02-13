using MongoDB.Bson;
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

        public async Task<Playlist> CreatePlaylistAsync()
        {
            return await Playlists.CreateAsync();
        }

        public async Task<Playlist> GetPlaylistAsync(ObjectId Id)
        {
            return await Playlists.GetAsync(Id);
        }

        public async Task DeletePlaylistAsync(ObjectId Id)
        {
            await Playlists.DeleteAsync(Id);
        }

        public async Task<Song> GetSongAsync(ObjectId Id)
        {
            return await Songs.GetAsync(Id);
        }

        public async Task<Opus> GetOpusStreamAsync(Song Song)
        {
            return await Songs.GetStreamAsync(Song);
        }

        private async Task ResyncAsync()
        {
            /// TODO: Delete unused songs. Automate if possible.
        }
    }
}