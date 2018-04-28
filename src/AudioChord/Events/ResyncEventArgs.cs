using System;

namespace AudioChord
{
    public class ResyncEventArgs : EventArgs
    {
        public int DeletedDesyncedFiles { get; }
        public int DeletedExpiredSongs { get; }

        internal ResyncEventArgs(int deletedDesyncedFiles, int deletedExpiredSongs)
        {
            DeletedDesyncedFiles = deletedDesyncedFiles;
            DeletedExpiredSongs = deletedExpiredSongs;
        }
    }
}