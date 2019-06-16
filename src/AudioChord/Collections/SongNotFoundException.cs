using System;

namespace AudioChord.Collections
{
    /// <summary>
    /// Thrown when the given song id does not exist
    /// </summary>
    public class SongNotFoundException : Exception
    {
        public SongNotFoundException(string message) : base(message)
        { }

        public SongNotFoundException(string message, Exception innerException) : base(message, innerException)
        { }
    }
}