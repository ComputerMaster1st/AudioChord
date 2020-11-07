using System;
using System.Net.Http;

namespace AudioChord.Extractors
{
    public class ExtractorConfiguration
    {
        public TimeSpan MaxSongDuration { get; set; } = TimeSpan.FromHours(1);
        public HttpClient? ImportedHttpClient { get; set; } = null;
    }
}