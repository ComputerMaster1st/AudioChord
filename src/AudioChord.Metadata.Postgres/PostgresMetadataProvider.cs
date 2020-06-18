using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Npgsql;
using NpgsqlTypes;

namespace AudioChord.Metadata.Postgres
{
    [PublicAPI]
    public class PostgresMetadataProvider : IMetadataProvider, ISupportsRandomMetadataRetrieval
    {
        private readonly PostgresConnectionFactory _factory;
        
        public PostgresMetadataProvider(PostgresConnectionFactory factory)
        {
            _factory = factory;
        }
        
        public async IAsyncEnumerable<SongMetadata> GetAllMetadataAsync()
        {
            await using NpgsqlConnection connection = await _factory.CreateOpenConnectionAsync();
            using NpgsqlCommand command = connection.CreateCommand();

            const string sql = @"
                SELECT id, title, duration, uploader, source 
                FROM song_metadata;";

            command.CommandText = sql;
            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                yield return MapMetadata(reader);
            }
        }

        public async Task<SongMetadata?> GetSongMetadataAsync(SongId id)
        {
            await using NpgsqlConnection connection = await _factory.CreateOpenConnectionAsync();
            using NpgsqlCommand command = connection.CreateCommand();

            const string sql = @"
                SELECT id, title, duration, uploader, source 
                FROM song_metadata 
                WHERE id = @songId";

            command.CommandText = sql;
            command.Parameters
                .AddWithValue("songId", NpgsqlDbType.Text, id.ToString());
            
            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return MapMetadata(reader);

            return default;
        }

        public async Task<SongMetadata?> RemoveSongMetadataAsync(SongId id)
        {
            await using NpgsqlConnection connection = await _factory.CreateOpenConnectionAsync();
            using NpgsqlCommand command = connection.CreateCommand();

            const string sql = @"
                DELETE FROM song_metadata 
                WHERE id = @songId
                    RETURNING *
                ";

            command.CommandText = sql;
            command.Parameters
                .AddWithValue("songId", NpgsqlDbType.Text, id.ToString());

            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return MapMetadata(reader);

            return default;
        }

        public async Task StoreSongMetadataAsync(ISong song)
        {
            await using NpgsqlConnection connection = await _factory.CreateOpenConnectionAsync();
            using NpgsqlCommand command = connection.CreateCommand();

            // Do not throw an exception if there's a conflict when inserting
            const string sql = @"
                INSERT INTO song_metadata (id, title, duration, uploader, source)
                VALUES (@songId, @title, @duration, @uploader, @source)
                ON CONFLICT (id)
                    DO UPDATE 
                    SET
                        title    = @title,
                        duration = @duration,
                        uploader = @uploader,
                        source   = @source 
                ";

            command.CommandText = sql;
            command.Parameters.AddWithValue("songId", NpgsqlDbType.Text, song.Metadata.Id.ToString());
            command.Parameters.AddWithValue("title", NpgsqlDbType.Text, song.Metadata.Title);
            command.Parameters.AddWithValue("duration", NpgsqlDbType.Interval, song.Metadata.Duration);
            command.Parameters.AddWithValue("uploader", NpgsqlDbType.Text, song.Metadata.Uploader);
            command.Parameters.AddWithValue("source", NpgsqlDbType.Text, song.Metadata.Source);

            await command.ExecuteNonQueryAsync();
        }

        public async IAsyncEnumerable<SongId> GetRandomSongMetadataAsync(long amount)
        {
            await using NpgsqlConnection connection = await _factory.CreateOpenConnectionAsync();
            using NpgsqlCommand command = connection.CreateCommand();

            const string sql = @"
                SELECT id FROM song_metadata 
                    TABLESAMPLE bernoulli(80) 
                ORDER BY random() 
                LIMIT 100
            ";

            command.CommandText = sql;
            await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                yield return SongId.Parse(reader.GetString(0));
            }
        }

        private SongMetadata MapMetadata(NpgsqlDataReader reader)
        {
            return new SongMetadata
            {
                Id = SongId.Parse(reader.GetString(0)),
                Title = reader.GetString(1),
                Duration = reader.GetTimeSpan(2),
                Uploader = reader.GetString(3),
                Source = reader.GetString(4)
            };
        }
    }
}