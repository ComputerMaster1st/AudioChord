using AudioChord.Caching.GridFS;
using Xunit;
using Moq;
using MongoDB.Driver;
using System.Threading.Tasks;
using System.IO;
using MongoDB.Driver.GridFS;
using System;
using System.Threading;

namespace AudioChord.Tests
{
    public class GridFSCacheTests
    {
        private Mock<IMongoDatabase> database;
        private Mock<IGridFSBucket<string>> bucket;
        private Mock<IMongoCollection<GridFSFileInfo<string>>> collection;

        private const string LAST_ACCESS_DATE_KEY = "lastAccessed";

        public GridFSCacheTests()
        {
            Mock<IMongoCollection<GridFSFileInfo<string>>> mockedFilesCollection = new Mock<IMongoCollection<GridFSFileInfo<string>>>();

            Mock<IMongoDatabase> mockedDatabase = new Mock<IMongoDatabase>();
            mockedDatabase
                .Setup(setup => setup.GetCollection<GridFSFileInfo<string>>(It.IsAny<string>(), It.IsAny<MongoCollectionSettings>()))
                .Returns(mockedFilesCollection.Object);

            Mock<IGridFSBucket<string>> mockedBucket = new Mock<IGridFSBucket<string>>();
            mockedBucket
                .Setup(setup => setup.Database)
                .Returns(mockedDatabase.Object);

            mockedBucket
                .Setup(setup => setup.Options)
                .Returns(new ImmutableGridFSBucketOptions());

            FilterDefinition<GridFSFileInfo<string>> definition = new FilterDefinitionBuilder<GridFSFileInfo<string>>()
                .Where(filter => filter.Metadata.GetValue(LAST_ACCESS_DATE_KEY, DateTime.Now) < DateTime.Now.AddMonths(-3));

            FilterDefinition<GridFSFileInfo<string>> second = new FilterDefinitionBuilder<GridFSFileInfo<string>>()
                .Where(filter => filter.Metadata.GetValue(LAST_ACCESS_DATE_KEY, DateTime.Now) < DateTime.Now.AddMonths(-3));

            Mock<IAsyncCursor<GridFSFileInfo<string>>> mockedCursor = new Mock<IAsyncCursor<GridFSFileInfo<string>>>();
            mockedCursor
                .Setup(setup => setup.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(false);

            mockedCursor
                .Setup(setup => setup.MoveNextAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(false));

            mockedBucket
                .Setup(setup => setup.Find(It.IsAny<FilterDefinition<GridFSFileInfo<string>>>(), It.IsAny<GridFSFindOptions<string>>(), It.IsAny<CancellationToken>()))
                .Returns(mockedCursor.Object);

            collection = mockedFilesCollection;
            database = mockedDatabase;
            bucket = mockedBucket;
        }

        [Fact]
        public async Task GridFSCache_CachesSong()
        {
            GridFSCache cache = new GridFSCache(bucket.Object);

            using(MemoryStream fakeStream = new MemoryStream())
            {
                SongId id = new SongId("TEST", "592952");

                Mock<ISong> fakeSong = new Mock<ISong>();
                fakeSong
                    .Setup(setup => setup.Id)
                    .Returns(id);

                fakeSong
                    .Setup(setup => setup.GetMusicStreamAsync())
                    .Returns(Task.FromResult<Stream>(fakeStream));

                await cache.CacheSongAsync(fakeSong.Object);

                (bool result, Stream stream) = await cache.TryFindCachedSongAsync(id);

                // Check that the cache returned the same song
                Assert.True(result);
                Assert.Same(fakeStream, stream);
            }
        }
    }
}
