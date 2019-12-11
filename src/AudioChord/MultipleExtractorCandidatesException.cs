using System;

namespace AudioChord
{
    /// <summary>
    /// Thrown when multiple extractors are found for the same url
    /// </summary>
    public class MultipleExtractorCandidatesException : Exception
    {
        public MultipleExtractorCandidatesException(string message)
            : base(message)
        { }
    }
}