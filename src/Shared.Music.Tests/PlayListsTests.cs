using System;
using Xunit;
using Shared.Music;
using Shared.Music.Collections.Models;

namespace Shared.Music.Tests
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
        public async void Playlist_CanCreate_ReturnsTrue()
        {
            Playlist playlist = await service.CreatePlaylistAsync();
            Assert.NotNull(playlist);
        }
    }
}
