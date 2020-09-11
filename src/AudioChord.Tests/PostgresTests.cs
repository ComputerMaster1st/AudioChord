using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AudioChord.Metadata;
using AudioChord.Metadata.Postgres;
using Xunit;

namespace AudioChord.Tests
{
    public class PostgresTests
    {
        private readonly IMetadataProvider _postgresProvider;
        
        public PostgresTests()
        {
            _postgresProvider = new PostgresMetadataProvider(new PostgresConnectionFactory(new ConnectionConfiguration
            {
                ConnectionString = "Server=127.0.0.1;Port=5432;Database=AudioChord;User Id=application;Password=development;"
            }));
        }
        
        [Fact]
        public async Task PostgresProviderCanRetrieveRows()
        {
            IEnumerable<SongMetadata> metadata = await _postgresProvider.GetAllMetadataAsync()
                .ToListAsync();
            
            Assert.NotEmpty(metadata);
            Assert.NotNull(metadata.First().Id);
        }

        [Fact]
        public async Task PostgresProviderCanRetrieveSingleInfo()
        {
            SongMetadata? metadata = await _postgresProvider.GetSongMetadataAsync(new SongId("YOUTUBE", "XFflwRnn2Ts"));
            
            Assert.NotNull(metadata);
            Assert.NotNull(metadata!.Id);
        }

        [Fact]
        public async Task PostgresProviderCanStoreBareNewMetadata()
        {
            SongMetadata metadata = new SongMetadata
            {
                Id = new SongId("TEST", "0001")
            };

            // Store the song
            await _postgresProvider.StoreSongMetadataAsync(new Song(metadata.Id, metadata, Stream.Null));
            
            // We should be able to retrieve it back
            SongMetadata? retrieved = await _postgresProvider.GetSongMetadataAsync(metadata.Id);
            Assert.NotNull(retrieved);
        }

        [Fact]
        public async Task PostgresProviderDoesNotFailOnInsertingExistingData()
        {
            SongMetadata metadata = new SongMetadata
            {
                Id = new SongId("TEST", "0001")
            };
            
            // Store the song
            await _postgresProvider.StoreSongMetadataAsync(new Song(metadata.Id, metadata, Stream.Null));
            
            // Store the song twice
            await _postgresProvider.StoreSongMetadataAsync(new Song(metadata.Id, metadata, Stream.Null));
            
            // We should be able to retrieve it back
            SongMetadata? retrieved = await _postgresProvider.GetSongMetadataAsync(metadata.Id);
            Assert.NotNull(retrieved);
        }

        [Fact]
        public async Task PostgresProviderUpdatesExistingDataOnInsert()
        {
            SongMetadata metadata = new SongMetadata
            {
                Id = new SongId("TEST", "0001"),
                Title = "My First Song!"
            };
            
            // Store the song
            await _postgresProvider.StoreSongMetadataAsync(new Song(metadata.Id, metadata, Stream.Null));

            SongMetadata updatedMetadata = new SongMetadata
            {
                Id = new SongId("TEST", "0001"),
                Title = "My Second Song!"
            };
            
            // Update the stored song
            await _postgresProvider.StoreSongMetadataAsync(new Song(metadata.Id, updatedMetadata, Stream.Null));
            
            // We should be able to retrieve it back
            SongMetadata? retrieved = await _postgresProvider.GetSongMetadataAsync(metadata.Id);
            Assert.NotNull(retrieved);
            Assert.Equal(updatedMetadata.Title, retrieved!.Title);
        }

        [Fact]
        public async Task PostgresProviderDeletesData()
        {
            SongMetadata metadata = new SongMetadata
            {
                Id = new SongId("TEST", "0001"),
                Title = "My First Song!"
            };
            
            // Store the song
            await _postgresProvider.StoreSongMetadataAsync(new Song(metadata.Id, metadata, Stream.Null));
            
            SongMetadata? deleted = await _postgresProvider.RemoveSongMetadataAsync(metadata.Id);
            Assert.NotNull(deleted);
        }
    }
}