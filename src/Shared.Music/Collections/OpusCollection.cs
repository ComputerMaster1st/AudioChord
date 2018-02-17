using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System.IO;
using System.Threading.Tasks;

namespace Shared.Music.Collections
{
    internal class OpusCollection
    {
        private GridFSBucket collection;

        internal OpusCollection(IMongoDatabase database)
        {
            collection = new GridFSBucket(database, new GridFSBucketOptions()
            {
                BucketName = "OpusData",
                ChunkSizeBytes = 2097152
            });
        }

        internal async Task<Stream> OpenOpusStreamAsync(ObjectId opusId)
        {
            return await collection.OpenDownloadStreamAsync(opusId);
        }

        internal async Task<ObjectId> StoreOpusStreamAsync(string Filename, Stream FfmpegStream)
        {
            return await collection.UploadFromStreamAsync(Filename, FfmpegStream);
        }
    }
}
