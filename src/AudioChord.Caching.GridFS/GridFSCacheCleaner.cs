using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Threading.Tasks;

namespace AudioChord.Caching.GridFS
{
    /// <summary>
    /// Responsible for cleaning the GridFS Cache when needed
    /// </summary>
    internal class GridFSCacheCleaner
    {
        private IGridFSBucket<string> cache;

        protected IMongoCollection<GridFSFileInfo<string>> files;

        /// <summary>
        /// The name of the gridfs metadata key that contains the date when the song was last acessed
        /// </summary>
        private const string LAST_ACCESS_DATE_KEY = "lastAccessed";

        public GridFSCacheCleaner(IGridFSBucket<string> cache)
        {
            this.cache = cache;
            files = cache.Database.GetCollection<GridFSFileInfo<string>>($"{cache.Options.BucketName}.files", null);
        }

        public async Task CleanExpiredCacheEntries()
        {
            FilterDefinitionBuilder<GridFSFileInfo<string>> builder = new FilterDefinitionBuilder<GridFSFileInfo<string>>();

            // Find all the songs that haven't been used for 3 months
            // TODO: How are we gonna upgrade all the songs?
            // BACKCOMPAT: If the song does not have the last used date in the metadata then use the current date
            FilterDefinition<GridFSFileInfo<string>> definition = builder
                .Where(filter => filter.Metadata.GetValue(LAST_ACCESS_DATE_KEY, DateTime.Now) < DateTime.Now.AddMonths(-3));

            // Delete a maximum of 100 songs per cleanup operation so we don't wait too long for returning a song
            foreach (GridFSFileInfo<string> info in cache.Find(definition, new GridFSFindOptions<string>() { Limit = 100 }).ToEnumerable())
            {
                await cache.DeleteAsync(info.Id);
            }
        }

        public void UpdateCacheTimestamp(SongId id)
        {
            var fieldDefinition = new StringFieldDefinition<GridFSFileInfo<string>, BsonDocument>($"metadata.{LAST_ACCESS_DATE_KEY}");

            var updateCommand = new UpdateDefinitionBuilder<GridFSFileInfo<string>>()
                .Set(fieldDefinition, GenerateGridFSMetadata());

            files.UpdateOne(filter => filter.Id == id.ToString(), updateCommand);
        }

        public BsonDocument GenerateGridFSMetadata()
            => new BsonDocument(LAST_ACCESS_DATE_KEY, DateTime.UtcNow);
    }
}