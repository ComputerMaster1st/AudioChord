using AudioChord.Collections;
using System.Threading.Tasks;

namespace AudioChord.Wrappers
{
    public class DiscordProcessorWrapper
    {
        private SongCollection songCollection;

        internal DiscordProcessorWrapper(SongCollection song)
        {
            songCollection = song;
        }

        /// <summary>
        /// Download song from Discord to database. (Note: Exceptions are to be expected.)
        /// </summary>
        /// <param name="url">The discord attachment url.</param>
        /// <param name="uploader">The discord username.</param>
        /// <param name="attachmentId">The discord attachment Id.</param>
        /// <param name="autoDownload">Automatically download if non-existent.</param>
        /// <returns>the newly downloaded <see cref="Song"/>.</returns>
        /// <exception cref="ArgumentNullException">Any of the parameters given is <see langword="null"/> or empty</exception>
        public Task<ISong> DownloadAsync(string url, string uploader, ulong attachmentId)
            => songCollection.DownloadFromDiscordAsync(url, uploader, attachmentId);
    }
}
