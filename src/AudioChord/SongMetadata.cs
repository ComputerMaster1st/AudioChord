using System;
using JetBrains.Annotations;

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

        #region Obsolete Properties

        [Obsolete("Replaced with SongMetadata.Duration")]
        public TimeSpan Length
        {
            get => Duration;
            set => Duration = value;
        }
        
        [Obsolete("Replaced with SongMetadata.Title")]
        public string Name
        {
            get => Title;
            set => Title = value;
        }

        #endregion
    }
}