using System.Threading.Tasks;
using AudioChord.Extractors;
using AudioChord.Extractors.Discord;
using Xunit;

namespace AudioChord.Tests
{
    public class DiscordExtractorTests
    {
        private const string DISCORD_URL = "https://cdn.discordapp.com/attachments/400706177618673666/414561033370468352/Neptune.mp3";
        private const ulong ATTACHMENT_ID = 414561033370468352;

        [Fact]
        public void DiscordExtractorCanRetrieveId()
        {
            // Setup
            DiscordExtractor extractor = new DiscordExtractor();
            
            // Assert
            Assert.True(extractor.CanExtract(DISCORD_URL));
        }

        [Fact]
        public void DiscordExtractorRetrievesCorrectId()
        {
            // Setup
            DiscordExtractor extractor = new DiscordExtractor();

            // Assert
            // This needs to be true
            Assert.True(extractor.TryExtractSongId(DISCORD_URL, out SongId id));
            
            // The SongId source id should be the same as the attachment id
            Assert.Equal(ATTACHMENT_ID, ulong.Parse(id.SourceId));
        }

        [Fact]
        public async Task DiscordExtractorCanExtractAudio()
        {
            // Setup
            DiscordExtractor extractor = new DiscordExtractor();
            ISong song = await extractor.ExtractAsync(DISCORD_URL, new ExtractorConfiguration());
            
            // Assert
            Assert.NotNull(song);
        } 
    }
}