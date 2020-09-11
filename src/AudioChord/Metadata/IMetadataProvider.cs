using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace AudioChord.Metadata
{
    /// <summary>
    /// handles storing and retrieving of Song Metadata
    /// </summary>
    [PublicAPI]
    public interface IMetadataProvider
    {
        /// <summary>
        /// Retrieve all <see cref="SongMetadata"/> in the storage medium
        /// </summary>
        /// <returns></returns>
        IAsyncEnumerable<SongMetadata> GetAllMetadataAsync();

        /// <summary>
        /// Retrieve one entry of <see cref="SongMetadata"/> or null
        /// </summary>
        /// <param name="id">The SongId to look for</param>
        /// <returns>null if the <see cref="SongMetadata"/> does not exist</returns>
        Task<SongMetadata?> GetSongMetadataAsync(SongId id);

        /// <summary>
        /// Remove <see cref="SongMetadata"/> with the given <see cref="SongId"/>
        /// </summary>
        /// <param name="id">The <see cref="SongId"/> of the song to remove</param>
        /// <returns></returns>
        Task<SongMetadata?> RemoveSongMetadataAsync(SongId id);
        
        /// <summary>
        /// Store <see cref="SongMetadata"/> in the provider
        /// </summary>
        /// <param name="song">The song to store</param>
        Task StoreSongMetadataAsync(ISong song);
    }
}