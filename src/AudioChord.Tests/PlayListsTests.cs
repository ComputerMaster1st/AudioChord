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
        public async Task Playlist_Create()
        {
            Playlist playlist = await service.CreatePlaylist();
            Assert.NotNull(playlist);
        }

        [Fact]
        public async Task Playlist_SaveSongYoutube()
        {
            Playlist p = await service.CreatePlaylist();
            ISong song = await service.Youtube.DownloadAsync(new Uri("https://www.youtube.com/watch?v=744AQ0rhdRk"));

            p.Songs.Add(song);

            await p.SaveAsync();
        }

        [Fact]
        public async void Playlist_SaveSongDiscord()
        {
            Playlist playlist = await service.CreatePlaylist();

            ISong song = await service.Discord.DownloadAsync("https://cdn.discordapp.com/attachments/400706177618673666/414561033370468352/Neptune.mp3", "ComputerMaster1st#6458", 414561033370468352);
            playlist.Songs.Add(song);

            await playlist.SaveAsync();
        }
    }
}
