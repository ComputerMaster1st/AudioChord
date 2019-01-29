using AudioChord.Caching.GridFS;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AudioChord.Tests
{
    public class PlaylistTests
    {
        private MusicService service;

        public PlaylistTests()
        {
            // Use the builder to allow to connect to database without authentication
            MongoUrlBuilder connectionStringBuilder = new MongoUrlBuilder
            {
                DatabaseName = "sharedmusic",
                Server = new MongoServerAddress("localhost")
            };

            MongoClient client = new MongoClient(connectionStringBuilder.ToMongoUrl());
            IMongoDatabase database = client.GetDatabase("sharedmusic");


            service = new MusicService(new MusicServiceConfiguration()
            {
                SongCacheFactory = () => new GridFSCache(database)
            });
        }

        [Fact]
        public void Playlist_Create()
        {
            Playlist playlist = new Playlist();
            Assert.NotNull(playlist);
        }

        [Fact]
        public async Task Playlist_SaveSongYoutube()
        {
            Playlist p = new Playlist();
            ISong song = await service.Youtube.DownloadAsync(new Uri("https://www.youtube.com/watch?v=744AQ0rhdRk"));

            p.Songs.Add(song.Id);

            await service.Playlist.UpdateAsync(p);

            Playlist p2 = await service.Playlist.GetPlaylistAsync(p.Id);

            Assert.NotNull(p2);
            Assert.NotNull(p2.Songs);
            Assert.NotEmpty(p2.Songs);
        }

        [Fact]
        public async Task Playlist_SaveSongDiscord()
        {
            Playlist playlist = new Playlist();

            ISong song = await service.Discord.DownloadAsync("https://cdn.discordapp.com/attachments/400706177618673666/414561033370468352/Neptune.mp3", "ComputerMaster1st#6458", 414561033370468352);
            playlist.Songs.Add(song.Id);

            await service.Playlist.UpdateAsync(playlist);

            Playlist p2 = await service.Playlist.GetPlaylistAsync(playlist.Id);

            Assert.NotNull(p2);
            Assert.NotNull(p2.Songs);
            Assert.NotEmpty(p2.Songs);
        }
    }
}
