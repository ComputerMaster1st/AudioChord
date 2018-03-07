using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace AudioChord.Tests
{
    public class PlaylistTests
    {
        private MusicService service;

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
            Assert.NotNull(playlist);
        }

        [Fact]
        public async void Playlist_SaveSongYoutube()
        {
            Playlist test = await service.CreatePlaylist();
            Playlist playlist = await service.GetPlaylistAsync(test.Id);
            string songId = await service.DownloadSongFromYouTubeAsync("https://www.youtube.com/watch?v=z91rnf-UBfM");
            playlist.Songs.Add(songId);
            await playlist.SaveAsync();
        }

        [Fact]
        public async void Playlist_SaveSongDiscord()
        {
            Playlist test = await service.CreatePlaylist();
            Playlist playlist = await service.GetPlaylistAsync(test.Id);
            string songId = await service.DownloadSongFromDiscordAsync("https://cdn.discordapp.com/attachments/400706177618673666/414561033370468352/Neptune.mp3", "ComputerMaster1st#6458", 414561033370468352);
            playlist.Songs.Add(songId);
            await playlist.SaveAsync();
        }

        [Fact]
        public async Task Song_ValidateMemoryDisposing()
        {
            GC.Collect();

            //test the task
            await UseMemory();

            //full GC agian
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private async Task UseMemory()
        {
            string songid = await service.DownloadSongFromYouTubeAsync("https://www.youtube.com/watch?v=z91rnf-UBfM");
            Song song = await service.GetSongAsync(songid);

            using (Stream stream = await song.GetMusicStreamAsync())
            {

            }
        }

        //[Fact]
        //public async void Song_GetStream()
        //{
        //    SongStream stream = await service.GetSongStreamAsync(songId2);
        //    Assert.NotNull(stream.MusicStream);
        //}

        //[Fact]
        //public async void Playlist_DeleteSong()
        //{
        //    Playlist playlist = await service.GetPlaylistAsync(playlistId);
        //    playlist.Songs.Remove(songId1);
        //    playlist.Songs.Remove(songId2);
        //    await playlist.SaveAsync();
        //}

        //[Fact]
        //public async void Playlist_Delete()
        //{
        //    await service.DeletePlaylistAsync(playlistId);
        //}

        //[Fact]
        //public async void Playlist_Resync()
        //{
        //    await service.Resync();
        //}
    }
}
