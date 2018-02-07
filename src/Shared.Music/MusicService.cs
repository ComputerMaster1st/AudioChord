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
            // Creates an empty playlist
            return await Playlists.CreateAsync();
        }

        public async Task<Playlist> GetPlaylistAsync(Guid Id)
        {
            // Get Specified Playlist
            return await Playlists.GetAsync(Id);
        }
    }
}

//    Public Async Function DeletePlaylistAsync(Id As Guid) As Task
//        'Delete Playlist
//        Await Playlists.DeleteAsync(Id)
//    End Function

//    Public Async Function AddSongToPlaylist() As Task
//        'Post Song To Playlist
//    End Function

//    Public Async Function DeleteSongFromPlaylistAsync() As Task
//        'Remove Song
//    End Function

//    Public Async Function GetSongAsync() As Task
//        'Get Song Meta Data
//        'Post Song To Process
//    End Function

//    Public Async Function GetSongAndOpusStreamAsync() As Task
//        'Get Song Opus Stream
//    End Function

//    Private Async Function ResyncRepositoryAsync() As Task
//        'Delete Song
//        'Automatic Resync
//    End Function
//End Class