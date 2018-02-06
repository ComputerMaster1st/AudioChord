﻿using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Shared.Music.Collections.Models;
using System.Threading.Tasks;

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

        public async Task<Opus> GetStreamAsync(Song song)
        {
            Opus stream = (Opus)song;
            stream.OpusStream = await bucket.OpenDownloadStreamAsync(song.OpusId);
            return stream;
        }
    }
}