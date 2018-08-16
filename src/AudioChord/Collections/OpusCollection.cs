using System;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AudioChord.Collections
{
    internal class OpusCollection
    {
        private GridFSBucket<string> bucket;

        internal OpusCollection(IMongoDatabase database)
        {
            bucket = new GridFSBucket<string>(database, new GridFSBucketOptions()
            {
                BucketName = "OpusData",
                ChunkSizeBytes = 4194304,

                // We don't use MD5 in our code
                DisableMD5 = true
            });
        }

        internal async Task StoreOpusStreamAsync(ISong song)
        {
            await bucket.UploadFromStreamAsync(song.Id.ToString(), $"{song.Id}.opus", await song.GetMusicStreamAsync());
        }

        internal async Task<Stream> OpenOpusStreamAsync(DatabaseSong song)
        {
            var output = await bucket.OpenDownloadStreamAsync(song.Id.ToString(), new GridFSDownloadOptions() { Seekable = true });
            return Stream.Synchronized(output);
        }

        internal Task DeleteAsync(SongId id)
        {
            return bucket.DeleteAsync(id.ToString());
        }

        internal async Task<double> TotalBytesUsedAsync()
        {
            double totalBytes = 0;

            var result = await bucket.FindAsync(FilterDefinition<GridFSFileInfo<string>>.Empty);
            await result.ForEachAsync(song => totalBytes += song.Length);

            return totalBytes;
        }

        internal async Task<IEnumerable<string>> GetAllOpusIdsAsync()
        {
            var result = await bucket.FindAsync(FilterDefinition<GridFSFileInfo<string>>.Empty);
            return (await result.ToListAsync())
                .ConvertAll(new Converter<GridFSFileInfo<string>, string>(fileInfo => { return fileInfo.Id; }));
        }
    }
}
