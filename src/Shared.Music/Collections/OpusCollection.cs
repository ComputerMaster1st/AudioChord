using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace Shared.Music.Collections
{
    internal class OpusCollection
    {
        private GridFSBucket bucket;

        internal OpusCollection(IMongoDatabase database)
        {
            bucket = new GridFSBucket(database, new GridFSBucketOptions() {
                BucketName = "OpusData",
                ChunkSizeBytes = 2097152
            });
        }
    }
}