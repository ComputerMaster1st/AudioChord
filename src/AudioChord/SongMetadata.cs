using System;
using JetBrains.Annotations;
using MongoDB.Bson.Serialization.Attributes;

namespace AudioChord
{
    [PublicAPI]
    public class SongMetadata
    {
        /// <summary>
        /// Title of this song
        /// </summary>
        public string Title { get; set; } = "Unknown title";
        
        /// <summary>
        /// Total duration of this song
        /// </summary>
        public TimeSpan Duration { get; set; } = TimeSpan.Zero;
        
        public string Uploader { get; set; } = "Unknown Uploader";
        
        public string Url { get; set; } = "No source given";
    }
}