using Xunit;
using Shared.Music.Collections.Models;
using MongoDB.Bson;

namespace Shared.Music.Tests
{
    public class PlaylistTests
    {
        private MusicService service;
        private ObjectId playlistId;
        private ObjectId songId1;
        private ObjectId songId2;

        public PlaylistTests()
        {
            service = new MusicService(new MusicServiceConfig()
            {
            });
        }

        [Fact]
        public async void Playlist_Create()
        {
            Playlist playlist = await service.CreatePlaylist();
            playlistId = playlist.Id;
            Assert.NotNull(playlist);
        }

        [Fact]
        public async void Playlist_SaveSongYoutube()
        {
            Playlist playlist = await service.GetPlaylistAsync(playlistId);
            songId1 = await service.DownloadSongFromYouTubeAsync("https://www.youtube.com/watch?v=z91rnf-UBfM");
            playlist.Songs.Add(songId1);
            await playlist.SaveAsync();
        }

        [Fact]
        public async void Playlist_SaveSongDiscord()
        {
            Playlist playlist = await service.GetPlaylistAsync(playlistId);
            songId2 = await service.DownloadSongFromDiscordAsync("https://cdn.discordapp.com/attachments/400706177618673666/414561033370468352/Neptune.mp3", "ComputerMaster1st#6458", 414561033370468352);
            playlist.Songs.Add(songId2);
            await playlist.SaveAsync();
        }

        [Fact]
        public async void Song_GetMetadata()
        {
            SongMeta meta = await service.GetSongMetadataAsync(songId1);
            Assert.NotNull(meta);
        }

        [Fact]
        public async void Song_GetStream()
        {
            SongStream stream = await service.GetSongStreamAsync(songId2);
            Assert.NotNull(stream.MusicStream);
        }

        [Fact]
        public async void Playlist_DeleteSong()
        {
            Playlist playlist = await service.GetPlaylistAsync(playlistId);
            playlist.Songs.Remove(songId1);
            playlist.Songs.Remove(songId2);
            await playlist.SaveAsync();
        }

        [Fact]
        public async void Playlist_Delete()
        {
            await service.DeletePlaylistAsync(playlistId);
        }

        //[Fact]
        //public async void Playlist_Resync()
        //{
        //    await service.Resync();
        //}
    }
}
