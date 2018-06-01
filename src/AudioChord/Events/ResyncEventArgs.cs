using System;

namespace AudioChord
{
    public class ResyncEventArgs : EventArgs
    {
        public DateTime StartedAt { get; }
        public int DeletedDesyncedFiles { get; }
        public int DeletedExpiredSongs { get; }
        public int ResyncedPlaylists { get; }

        internal ResyncEventArgs(DateTime startedAt, int deletedDesyncedFiles, int deletedExpiredSongs, int resyncedPlaylists)
        {
            StartedAt = startedAt;
            DeletedDesyncedFiles = deletedDesyncedFiles;
            DeletedExpiredSongs = deletedExpiredSongs;
            ResyncedPlaylists = resyncedPlaylists;
        }
    }
}