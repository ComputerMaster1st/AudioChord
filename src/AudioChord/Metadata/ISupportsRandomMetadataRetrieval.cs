using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace AudioChord.Metadata
{
    /// <summary>
    /// Handles requesting a random amount of song metadata
    /// </summary>
    [PublicAPI]
    public interface ISupportsRandomMetadataRetrieval
    {
        /// <summary>
        /// Retrieve a random set of <see cref="SongId"/>'s
        /// </summary>
        /// <param name="amount">The exact amount of random entries to retrieve</param>
        /// <returns>An Enumerable with random <see cref="SongId"/>'s</returns>
        Task<IEnumerable<SongId>> GetRandomSongMetadataAsync(long amount);
    }
}