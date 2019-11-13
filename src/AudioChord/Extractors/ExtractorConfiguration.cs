using System;

namespace AudioChord.Extractors
{
    public class ExtractorConfiguration
    {
        public TimeSpan MaxSongDuration { get; set; } = TimeSpan.FromHours(1);
    }
}