using System;
using System.IO;
using System.Threading.Tasks;

namespace AudioChord.Caching.FileSystem
{
    /// <summary>
    /// Cache songs to the local filesystem
    /// </summary>
    public class FileSystemCache : ISongCache
    {
        /// <summary>
        /// The path to the directory to cache files in.
        /// </summary>
        private readonly string storageLocation;
        /// <summary>
        /// Cleaner responsible for cleaning the cache
        /// </summary>
        private readonly FileSystemCacheCleaner _cleaner;

        /// <summary>
        /// Create a new filesystem cache that uses a folder to store songs
        /// </summary>
        /// <param name="storagePath">The path of the folder to store songs into</param>
        /// <exception cref="ArgumentException">The given <paramref name="storagePath"/> does not exist or is not a directory</exception>
        public FileSystemCache(string storagePath)
        {
            // Check if the path exists
            if (!Directory.Exists(storagePath))
                throw new ArgumentException($"The path '{storagePath}' does not exist or is inaccessible");

            storageLocation = storagePath;
            _cleaner = new FileSystemCacheCleaner(storagePath);
        }

        public async Task CacheSongAsync(ISong song)
        {
            // Clean the cache
            _cleaner.CleanExpiredEntries();
            
            
            string fileLocation = Path.Combine(storageLocation, $"{song.Id}.opus");

            if (!File.Exists(fileLocation))
            {
                try
                {
                    // Open a new file with shared read access, throws an exception if the file already exists
                    using (FileStream newFile = new FileStream(fileLocation, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read))
                    {
                        Stream stream = await song.GetMusicStreamAsync();

                        // Copy the contents of the music stream to the file
                        await stream.CopyToAsync(newFile);
                    }
                }
                catch (IOException)
                {
                    // If this is thrown then the File already exists. Ignore the exception...
                }
            }
        }

        public Task<(bool success, Stream result)> TryFindCachedSongAsync(SongId id)
        {
            string fileLocation = Path.Combine(storageLocation, $"{id}.opus");

            if (File.Exists(fileLocation))
            {
                return Task.FromResult<(bool, Stream)>((true, File.OpenRead(fileLocation)));
            }
            else
            {
                return Task.FromResult<(bool, Stream)>((false, null));
            }
        }
    }
}
