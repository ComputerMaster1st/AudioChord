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
        private readonly string _storageLocation;
        
        /// <summary>
        /// Cleaner responsible for cleaning the cache
        /// </summary>
        private readonly FileSystemCacheCleaner _cleaner;

        /// <summary>
        /// Create a new filesystem cache that uses a directory to store songs
        /// </summary>
        /// <param name="storagePath">The path of the directory to store songs into</param>
        /// <exception cref="DirectoryNotFoundException">The given <paramref name="storagePath"/> does not exist or is not a directory</exception>
        public FileSystemCache(string storagePath)
        {
            // Check if the path exists
            if (!Directory.Exists(storagePath))
                throw new DirectoryNotFoundException($"The path '{storagePath}' does not exist or is inaccessible");

            _storageLocation = storagePath;
            _cleaner = new FileSystemCacheCleaner(storagePath);
        }

        /// <summary>
        /// Cache the given song to the cache
        /// </summary>
        /// <param name="song">The song to cache</param>
        public async Task CacheSongAsync(ISong song)
        {
            // Clean the cache
            _cleaner.CleanExpiredEntries();
            
            string fileLocation = Path.Combine(_storageLocation, $"{song.Id}.opus");

            if (!File.Exists(fileLocation))
            {
                try
                {
                    // Open a new file with shared read access, throws an exception if the file already exists
                    using (FileStream newFile = new FileStream(fileLocation, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.Read))
                    {
                        Stream stream = await song.GetMusicStreamAsync();

                        // Copy the contents of the music stream to the file and write to disk
                        await stream.CopyToAsync(newFile);
                        await newFile.FlushAsync();
                        
                        // Update the database
                        _cleaner.InsertEntry(song.Id);
                    }
                }
                catch (IOException)
                {
                    // If this is thrown then the File already exists. Ignore the exception...
                }
            }
        }

        /// <summary>
        /// Attempt to open a FileStream to an existing file in the cache
        /// </summary>
        /// <param name="id">The Id of the cached song that we need to stream</param>
        /// <returns>true or false with an open stream or default</returns>
        public Task<(bool success, Stream result)> TryFindCachedSongAsync(SongId id)
        {
            string fileLocation = Path.Combine(_storageLocation, $"{id}.opus");

            if (!File.Exists(fileLocation))
                return Task.FromResult<(bool, Stream)>((false, default));
            
            try
            {
                // Try to open a FileStream of the cached file
                // The file cannot be deleted while the stream is open. To allow deletion you need to modify the FileShare options
                FileStream cachedSongStream = new FileStream(fileLocation, FileMode.Open, FileAccess.Read, FileShare.Read);
                
                // Update the timestamp in the database
                _cleaner.UpdateEntry(id);
                
                return Task.FromResult<(bool, Stream)>((true, cachedSongStream));
            }
            catch (FileNotFoundException)
            {
                // The file does not exist and we only wanna open the file. Return failure
                return Task.FromResult<(bool, Stream)>((false, default));
            }
            catch (IOException)
            {
                // TODO: Log the error?
                return Task.FromResult<(bool, Stream)>((false, default));
            }
        }
    }
}
