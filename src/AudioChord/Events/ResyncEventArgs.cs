using System;

namespace AudioChord
{
    public class ResyncEventArgs : EventArgs
    {
        public int DeletedDesyncedFiles { get; }
        public int DeletedExpiredSongs { get; }
        public int ResyncedPlaylists { get; }

        internal ResyncEventArgs(int deletedDesyncedFiles, int deletedExpiredSongs, int resyncedPlaylists)
        {
            DeletedDesyncedFiles = deletedDesyncedFiles;
            DeletedExpiredSongs = deletedExpiredSongs;
            ResyncedPlaylists = resyncedPlaylists;
        }
    }
}