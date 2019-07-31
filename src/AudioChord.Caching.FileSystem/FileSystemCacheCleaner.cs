using System;
using System.Collections.Generic;
using System.IO;
using System.Data.SQLite;
using System.Net;

namespace AudioChord.Caching.FileSystem
{
    /// <summary>
    /// Cleans up the FileSystem of expired cache entries
    /// </summary>
    public class FileSystemCacheCleaner
    {
        private readonly string _cleaningFolder;
        private readonly string _connectionString;

        private const string DATABASE_FILE_NAME = "songdb.db";
        
        public FileSystemCacheCleaner(string cleanLocation)
        {
            _cleaningFolder = cleanLocation;
            _connectionString = $"Data Source={Path.Combine(cleanLocation, DATABASE_FILE_NAME)}; Journal Mode=wal;";
            
            // Create the database if it doesnt exist
            CreateDatabase();
        }

        /// <summary>
        /// Clean expired files in the cache
        /// </summary>
        public void CleanExpiredEntries()
        {
            // Query the database for expired entries
            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                List<string> toDelete = new List<string>(50);
                
                command.CommandText =  @"
                        SELECT filename 
                        FROM song_expiry 
                        WHERE expiration_date > datetime('now') limit 50
                                        ";

                connection.Open();
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.NextResult())
                    {
                        reader.GetString(0);
                    }
                }
                
                // Reset the command to use for deletion of entries in the db
                command.Reset();

                using (SQLiteTransaction transaction = connection.BeginTransaction())
                {
                    command.CommandText =  @"
                        DELETE FROM song_expiry 
                        WHERE filename = @name
                                        ";

                    foreach (string filename in toDelete)
                    {
                        string path = Path.Combine(_cleaningFolder, $"{filename}.opus");
                        if (File.Exists(path))
                        {
                            try
                            {
                                File.Delete(path);
                                // After this point file deletion is successful, delete row
                                command.Parameters.AddWithValue("@name", filename);
                                command.ExecuteNonQuery();
                            }
                            catch (IOException)
                            {
                                // Deletion was not successful, maybe it's still in use?
                                // TODO: log the exception?
                            }
                        }
                        else
                        {
                            // The file is already deleted, the database is out of sync. delete the row
                            command.Parameters.AddWithValue("@name", filename);
                            command.ExecuteNonQuery();
                        }
                    }
                    
                    // Commit to the database
                    transaction.Commit();
                }
            }
        }

        public void UpdateEntry(SongId id)
        {
            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                command.CommandText =  @"
                        update song_expiry
                        set expiration_date = DATETIME('now', '+3 months')
                        where filename = @name;
                                        ";

                command.Parameters.AddWithValue("@name", id.ToString());
                
                connection.Open();
                command.ExecuteNonQuery();
            }           
        }

        public void InsertEntry(SongId id)
        {
            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                command.CommandText =  @"
                        insert into song_expiry (filename, expiration_date) 
                        values (@name, datetime('now'))
                                        ";

                command.Parameters.AddWithValue("@name", id.ToString());
                
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private void CreateDatabase()
        {
            using (SQLiteConnection connection = new SQLiteConnection(_connectionString))
            using (SQLiteCommand command = new SQLiteCommand(connection))
            {
                command.CommandText =  @"
                        create table if not exists song_expiry
                        (
                            filename TEXT not null
                                constraint song_expiry_pk
                                    primary key,
                            expiration_date DATETIME not null
                        );

                        create unique index song_expiry_filename_uindex
                            on song_expiry (filename);
                                        ";
                
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}