using MongoDB.Driver;
using System;
using System.Linq;

namespace AudioChord
{
    /// <summary>
    /// A unique identifier for songs created by using the source and source Id
    /// </summary>
    public class SongId
    {
        /// <summary>
        /// The name of the source
        /// </summary>
        public string ProcessorId { get; private set; }

        /// <summary>
        /// The unique identifier the source had for this song
        /// </summary>
        public string SourceId { get; private set; }

        public SongId(string source, string sourceId)
        {
            if (string.IsNullOrWhiteSpace(source))
                throw new ArgumentException("source cannot be empty");

            if (string.IsNullOrWhiteSpace(source))
                throw new ArgumentException("id cannot be empty");

            //always use upper case for processor ids
            ProcessorId = source.ToUpper();
            SourceId = sourceId;
        }

        public static SongId Parse(string id)
        {
            // We need at least 3 characters to be able to parse an identifier (1 char Processor id + # + 1 char source id)
            if (id.Length < 3)
                throw new FormatException("Invalid songId identifier (string too short)");

            // String should contain only one instance of the '#' character
            if(id.Count(str => str == '#') != 1)
                throw new FormatException("Invalid songId identifier (invalid '#' count)");

            // String should not begin or end with '#'
            if(id.First() == '#' || id.Last() == '#')
                throw new FormatException("Invalid songId identifier (string should not start or end with '#')");

            int bangPosition = id.IndexOf('#');

            // Passed all constraints, parse the Id
            return new SongId(id.Substring(0, bangPosition), id.Substring(bangPosition + 1));
        }

        public override string ToString()
        {
            return $"{ProcessorId}#{SourceId}";
        }

        public override bool Equals(object obj)
        {
            //if its not the same object it fails
            if(!(obj is SongId id))
                return false;

            //if both ids are the same its the same object
            return id.ProcessorId == ProcessorId && id.SourceId == SourceId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ProcessorId, SourceId);
        }
    }
}
