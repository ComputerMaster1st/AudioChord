using System;
using JetBrains.Annotations;

namespace AudioChord
{
    [PublicAPI]
    public class SongMetadata
    {
        public SongId Id { get; set; }
        
        /// <summary>
        /// Title of this song
        /// </summary>
        public string Title { get; set; } = "Unknown title";
        
        /// <summary>
        /// Total duration of this song
        /// </summary>
        public TimeSpan Duration { get; set; } = TimeSpan.Zero;
        
        public string Uploader { get; set; } = "Unknown Uploader";
        
        public string Source { get; set; } = "No source given";
    }
}