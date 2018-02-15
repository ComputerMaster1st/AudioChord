using System;
using Xunit;
using Shared.Music;
using Shared.Music.Collections.Models;
using MongoDB.Bson;

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
            Playlist playlist = await service.CreatePlaylist();
            Assert.NotNull(playlist);
        }

        [Fact]
        public async void Playlist_CanSaveSong_ReturnsTrue()
        {
            Playlist playlist = await service.CreatePlaylist();
            playlist.Songs.Add(new Song("test", TimeSpan.Zero, "test", ObjectId.GenerateNewId()));
            await playlist.Save();
        }
    }
}
