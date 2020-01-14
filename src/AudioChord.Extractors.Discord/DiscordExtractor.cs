using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AudioChord.Processors;
using JetBrains.Annotations;

namespace AudioChord.Extractors.Discord
{
    [PublicAPI]
    public class DiscordExtractor : IAudioExtractor
    {
        private readonly FFprobeMetadataExtractor _extractor;

        private const string DISCORD_CDN_HOST = "cdn.discordapp.com";
        private const string PROCESSOR_PREFIX = "DISCORD";
        
        public DiscordExtractor()
        {
            _extractor = new FFprobeMetadataExtractor();
        }
        
        public bool CanExtract(string url)
        {
            // Example: https://cdn.discordapp.com/attachments/400706177618673666/414561033370468352/Neptune.mp3
            // Assuming that the first id is a channel id, the second one is an attachment id.
            
            if(!Uri.TryCreate(url, UriKind.Absolute, out Uri result))
                return false;

            if (result.Host != DISCORD_CDN_HOST)
                return false;
            
            // We need to sanitize the segments by removing the '/' characters
            string[] sanitizedSegments = result.Segments
                .Select(segment => segment.Replace("/", string.Empty))
                .Where(segment => segment.Length > 0)
                .ToArray();
            
            if(sanitizedSegments.Length != 4)
                return false;

            if (sanitizedSegments[0] != "attachments")
                return false;

            // Validate if the segments could be ids
            if (!ulong.TryParse(sanitizedSegments[1], out _))
                return false;
            
            if (!ulong.TryParse(sanitizedSegments[2], out _))
                return false;
            
            // There's a high chance of being able to extract the id
            return true;
        }

        public bool TryExtractSongId(string url, out SongId id)
        {
            if (!CanExtract(url))
            {
                id = default;
                return false;
            }
            
            // We need to sanitize the segments by removing the '/' characters
            string[] sanitizedSegments = new Uri(url).Segments
                .Select(segment => segment.Replace("/", string.Empty))
                .Where(segment => segment.Length > 0)
                .ToArray();
            
            // Grab the last id from the message
            id = new SongId(PROCESSOR_PREFIX, sanitizedSegments[2]);
            return true;
        }

        /// <summary>
        /// Extracts audio and metadata of an song from a discord attachment url
        /// </summary>
        /// <param name="url">The url to attempt to extract from</param>
        /// <param name="configuration">The configuration that the extractor needs to validate</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">The given url cannot be handled by this extractor</exception>
        /// <exception cref="InvalidOperationException">Cannot find corresponding user message for attachment url</exception>
        /// <exception cref="Exception"></exception>
        public async Task<ISong> ExtractAsync(string url, ExtractorConfiguration configuration)
        {
            // TODO: Honor the configuration object passed to the extractor
            if(!CanExtract(url))
                throw new ArgumentException(
                    $"url '{url}' cannot be extracted. This extractor does not know how to handle it",
                    nameof(url)
                );

            SongMetadata metadata = await _extractor.GetMetadataAsync(url);

            using (HttpClient http = new HttpClient())
            using (Stream httpStream = await http.GetStreamAsync(url))
            {
                MemoryStream memoryStream = new MemoryStream();
                await httpStream.CopyToAsync(memoryStream);

                if(!TryExtractSongId(url, out SongId id))
                    throw new Exception("Failed to extract an SongId from the Url");
                
                return new Song(id, metadata, memoryStream);
            }
        }
    }
}