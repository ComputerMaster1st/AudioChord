using System;

namespace AudioChord.Events
{
    public class ProcessedSongEventArgs : EventArgs
    {
        public string SongId { get; }
        public string SongName { get; }
        public ulong GuildId { get; }
        public ulong TextChannelId { get; }
        public int SongQueuePosition { get; }
        public int SongQueueLength { get; }

        internal ProcessedSongEventArgs(string songId, string songName, ulong guildId, ulong textChannelId, int songQueuePosition, int songQueueLength)
        {
            SongId = songId;
            SongName = songName;
            GuildId = guildId;
            TextChannelId = textChannelId;
            SongQueuePosition = songQueuePosition;
            SongQueueLength = songQueueLength;
        }
    }
}
