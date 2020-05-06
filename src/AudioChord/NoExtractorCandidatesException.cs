using System;

namespace AudioChord
{
    /// <summary>
    /// Thrown when multiple extractors are found for the same url
    /// </summary>
    public class NoExtractorCandidatesException : Exception
    {
        public NoExtractorCandidatesException(string message)
            : base(message)
        { }
    }
}