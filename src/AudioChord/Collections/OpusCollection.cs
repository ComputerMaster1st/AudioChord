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
                ChunkSizeBytes = 2097152
            });
        }

        internal async Task<ObjectId> MatchMD5Async(string MD5Hash)
        {
            List<GridFSFileInfo> result = await (await collection.FindAsync(FilterDefinition<GridFSFileInfo>.Empty)).ToListAsync();
            foreach (GridFSFileInfo fileInfo in result)
                if (fileInfo.MD5 == MD5Hash)
                    return fileInfo.Id;
            return ObjectId.Empty;
        }

        internal async Task<ObjectId> StoreOpusStreamAsync(string Filename, Stream FfmpegStream)
        {
            return await collection.UploadFromStreamAsync(Filename, FfmpegStream);
        }

        internal async Task<Stream> OpenOpusStreamAsync(ObjectId opusId)
        {
            var output = await collection.OpenDownloadStreamAsync(opusId);
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
    }
}
