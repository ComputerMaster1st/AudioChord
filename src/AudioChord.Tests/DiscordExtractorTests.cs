using System.Threading.Tasks;
using AudioChord.Extractors;
using AudioChord.Extractors.Discord;
using Discord;
using Discord.Rest;
using Xunit;

namespace AudioChord.Tests
{
    public class DiscordExtractorTests
    {
        private const string DISCORD_URL = "https://cdn.discordapp.com/attachments/400706177618673666/414561033370468352/Neptune.mp3";
        private const ulong ATTACHMENT_MESSAGE_ID = 414561033370468352;

        private readonly IDiscordClient _client;

        public DiscordExtractorTests()
        {
            DiscordRestClient client = new DiscordRestClient();
            
            client.LoginAsync(TokenType.Bot, System.Environment.GetEnvironmentVariable("DISCORD_TEST_BOT_TOKEN"))
                .GetAwaiter().GetResult();

            _client = client;
        }
        
        [Fact]
        public void DiscordExtractorCanRetrieveId()
        {
            // Setup
            DiscordExtractor extractor = new DiscordExtractor(_client);
            
            // Assert
            Assert.True(extractor.CanExtract(DISCORD_URL));
        }

        [Fact]
        public void DiscordExtractorRetrievesCorrectId()
        {
            // Setup
            DiscordExtractor extractor = new DiscordExtractor(_client);

            // Assert
            // This needs to be true
            Assert.True(extractor.TryExtractSongId(DISCORD_URL, out SongId id));
            
            // The SongId source id should be the same as the message id
            Assert.Equal(ATTACHMENT_MESSAGE_ID, ulong.Parse(id.SourceId));
        }

        [Fact]
        public async Task DiscordExtractorCanExtractAudio()
        {
            // Setup
            DiscordExtractor extractor = new DiscordExtractor(_client);
            ISong song = await extractor.ExtractAsync(DISCORD_URL, new ExtractorConfiguration());
            
            // Assert
            Assert.NotNull(song);
        } 
    }
}