using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AudioChord.Collections
{
    internal class OpusCollection
    {
        private GridFSBucket collection;

        internal OpusCollection(IMongoDatabase database)
        {
            collection = new GridFSBucket(database, new GridFSBucketOptions()
            {
                BucketName = "OpusData",
                ChunkSizeBytes = 4194304
            });
        }

        internal async Task<ObjectId> StoreOpusStreamAsync(string Filename, Stream FfmpegStream)
        {
            return await collection.UploadFromStreamAsync(Filename, FfmpegStream);
        }

        internal async Task<Stream> OpenOpusStreamAsync(ObjectId opusId)
        {
            var output = await collection.OpenDownloadStreamAsync(opusId, new GridFSDownloadOptions() { Seekable = true });
            return Stream.Synchronized(output);
        }

        internal async Task DeleteAsync(ObjectId opusId)
        {
            await collection.DeleteAsync(opusId);
        }

        internal async Task<double> TotalBytesUsedAsync()
        {
            var result = await collection.FindAsync(FilterDefinition<GridFSFileInfo>.Empty);
            List<GridFSFileInfo> fileList = await result.ToListAsync();
            double totalBytes = 0;

            foreach (GridFSFileInfo fileInfo in fileList)
                totalBytes += fileInfo.Length;

            return totalBytes;
        }

        internal async Task<IEnumerable<ObjectId>> GetAllOpusIdsAsync()
        {
            List<ObjectId> opusIds = new List<ObjectId>();
            var result = await collection.FindAsync(FilterDefinition<GridFSFileInfo>.Empty);
            List<GridFSFileInfo> fileList = await result.ToListAsync();

            foreach (GridFSFileInfo fileInfo in fileList) opusIds.Add(fileInfo.Id);

            return opusIds;
        }
    }
}
