using System;

namespace AudioChord.Events
{
    public class ProcessedSongEventArgs : EventArgs
    {
        public string SongId { get; }

        /// <summary>
        /// This will be null if an error occurs
        /// </summary>
        public string SongName { get; }

        public ulong GuildId { get; }
        public ulong TextChannelId { get; }
        public int SongsLeftToProcess { get; }
        public int SongInQueue { get; }

        internal ProcessedSongEventArgs(string songId, string songName, ulong guildId, ulong textChannelId, int songsLeftToProcess, int songsInQueue)
        {
            SongId = songId;
            SongName = songName;
            GuildId = guildId;
            TextChannelId = textChannelId;
            SongsLeftToProcess = songsLeftToProcess;
            SongInQueue = songsInQueue;
        }
    }
}
