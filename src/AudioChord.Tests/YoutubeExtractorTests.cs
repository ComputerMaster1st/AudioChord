using System.Threading.Tasks;
using AudioChord.Extractors;
using Xunit;

namespace AudioChord.Tests
{
    public class YoutubeExtractorTests
    {
        private readonly IAudioExtractor _extractor = new YouTubeExtractor();

        [Fact]
        public void CanExtractYoutubeVideo()
        {
            Assert.True(_extractor.CanExtract("https://www.youtube.com/watch?v=H7kUQjdZx_E"));
        }
        
        [Fact]
        public async Task SuccessfullyExtractsYoutubeVideo()
        {
            ISong song = await _extractor.ExtractAsync("https://www.youtube.com/watch?v=H7kUQjdZx_E", new ExtractorConfiguration());
            Assert.NotNull(song);
        }
    }
}