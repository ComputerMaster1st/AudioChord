using System;
using AudioChord.Collections;
using System.Threading.Tasks;

namespace AudioChord.Wrappers
{
    public class DiscordProcessorWrapper
    {
        private readonly SongCollection _songCollection;

        internal DiscordProcessorWrapper(SongCollection song)
        {
            _songCollection = song;
        }

        /// <summary>
        /// Download song from Discord to database. (Note: Exceptions are to be expected.)
        /// </summary>
        /// <param name="url">The discord attachment url.</param>
        /// <param name="uploader">The discord username.</param>
        /// <param name="attachmentId">The discord attachment Id.</param>
        /// <returns>the newly downloaded <see cref="Song"/>.</returns>
        /// <exception cref="ArgumentNullException">Any of the parameters given is <see langword="null"/> or empty</exception>
        public Task<ISong> DownloadAsync(string url, string uploader, ulong attachmentId)
            => _songCollection.DownloadFromDiscordAsync(url, uploader, attachmentId);
    }
}