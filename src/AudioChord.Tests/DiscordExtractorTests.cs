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

        private readonly DiscordExtractor _extractor;

        public DiscordExtractorTests()
        {
            _extractor = new DiscordExtractor();
        }

        [Fact(DisplayName = "Discord Extractor recognizes valid discord attachment Url")]
        public void DiscordExtractorCanRetrieveId()
        {
            // Assert
            Assert.True(_extractor.CanExtract(DISCORD_URL));
        }

        [Fact(DisplayName = "Discord Extractor generates the correct id from an Url")]
        public void DiscordExtractorRetrievesCorrectId()
        {
            // Assert
            Assert.True(_extractor.TryExtractSongId(DISCORD_URL, out SongId id));
            
            // The SongId source id should be the same as the attachment id
            Assert.Equal(ATTACHMENT_ID, ulong.Parse(id.SourceId));
        }

        [Fact(DisplayName = "Discord Extractor can extract audio from a valid discord link")]
        public async Task DiscordExtractorCanExtractAudio()
        {
            // Setup
            ISong song = await _extractor.ExtractAsync(DISCORD_URL, new ExtractorConfiguration());
            
            // Assert
            Assert.NotNull(song);
            Assert.Equal(song.Metadata.Url, DISCORD_URL);
        } 
    }
}