using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Shared.Music.Providers.Storage.GridFS
{
    internal class GridFSProvider : IStorageProvider
    {
        private GridFSBucket bucket;

        public GridFSProvider(IMongoDatabase database)
        {
            bucket = new GridFSBucket(database, new GridFSBucketOptions()
            {
                BucketName = "OpusFiles",
                ChunkSizeBytes = 2097152
            });
        }

        public Task<Playlist> GetPlaylistAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public async Task<Stream> GetSongAsync(Guid id)
        {
            return await bucket.OpenDownloadStreamByNameAsync($"{id}.opus");
        }
    }
}
