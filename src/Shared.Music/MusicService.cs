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
        private PlaylistCollection Playlists;
        private SongCollection Songs;
        private OpusCollection opusCollection;

        public MusicService(MusicServiceConfig config)
        {
            MongoClient client = new MongoClient($"mongodb://{config.Username}:{config.Password}@{config.Hostname}:27017/sharedmusic");
            IMongoDatabase database = client.GetDatabase("sharedmusic");

            Playlists = new PlaylistCollection(database.GetCollection<Playlist>(typeof(Playlist).Name));
            Songs = new SongCollection(database);
            opusCollection = new OpusCollection(database);
        }

        public Playlist CreatePlaylist()
        {
            return Playlists.Create();
        }

        public async Task<Playlist> GetPlaylistAsync(ObjectId PlaylistId)
        {
            return await Playlists.GetAsync(PlaylistId);
        }

        public async Task DeletePlaylistAsync(ObjectId Id)
        {
            await Playlists.DeleteAsync(Id);
        }

        public async Task<Song> GetSongAsync(ObjectId Id)
        {
            return await Songs.GetSongAsync(Id);
        }

        public async Task<Opus> GetOpusStreamAsync(Song song)
        {
            song.LastAccessed = DateTime.Now;
            await Songs.UpdateSongAsync(song);
            Opus opus = await opusCollection.OpenOpusStreamAsync(song);
            return opus;
        }

        private async Task ResyncAsync()
        {
            /// TODO: Delete unused songs. Automate if possible.
        }
    }
}