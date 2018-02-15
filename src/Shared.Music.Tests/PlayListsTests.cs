using System;
using Xunit;
using Shared.Music;
using Shared.Music.Collections.Models;
using MongoDB.Bson;
using Shared.Music.Processors;
using System.IO;

namespace Shared.Music.Tests
{
    public class PlaylistTests
    {
        private MusicService service;

        //public PlaylistTests()
        //{
        //    service = new MusicService(new MusicServiceConfig()
        //    {
        //    });
        //}

        [Fact]
        public void Playlist_CanCreate_ReturnsTrue()
        {
            Playlist playlist = service.CreatePlaylist();
            Assert.NotNull(playlist);
        }

        [Fact]
        public async void Playlist_CanSaveSong_ReturnsTrue()
        {
            Playlist playlist = service.CreatePlaylist();
            playlist.Songs.Add(new Song("test", TimeSpan.Zero, "test", ObjectId.GenerateNewId()));
            await playlist.SaveAsync();
        }

        [Fact]
        public async void FFMpeg_MemoryStream_ReturnsFile()
        {
            FFMpegEncoder encoder = new FFMpegEncoder(@"C:\Users\ComputerMaster1st\Music\Believe.mp3");
            var stream = await encoder.ProcessAsync();
            var opusTest = File.OpenWrite(@"C:\Users\ComputerMaster1st\Desktop\Believe.mp3");
            await stream.CopyToAsync(opusTest);
            opusTest.Close();
        }
    }
}
