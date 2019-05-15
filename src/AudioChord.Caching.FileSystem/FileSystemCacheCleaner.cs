using System;
using System.IO;

namespace AudioChord.Caching.FileSystem
{
    /// <summary>
    /// Cleans up the FileSystem of expired cache entries
    /// </summary>
    public class FileSystemCacheCleaner
    {
        private readonly string _cleaningFolder;
        
        public FileSystemCacheCleaner(string cleanLocation)
        {
            _cleaningFolder = cleanLocation;
        }

        /// <summary>
        /// Clean expired files in the cache
        /// </summary>
        public void CleanExpiredEntries()
        {
            if (Directory.Exists(_cleaningFolder))
            {
                var info = new DirectoryInfo(_cleaningFolder);
                
                // What happens if the amount of files is very large? does it take a long time?
                foreach (FileInfo fileInfo in info.EnumerateFiles("*.opus"))
                {
                    if (fileInfo.CreationTimeUtc < DateTime.UtcNow.AddMonths(-3))
                    {
                        // Try to delete the file
                        try
                        {
                            fileInfo.Delete();
                        }
                        catch (IOException)
                        {
                            // The file is in use, Ignore for now and try to clean it up later
                        }
                    }
                }
            }
        }
    }
}