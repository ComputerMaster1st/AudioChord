using System;

namespace AudioChord.Events
{
    public class ProcessedSongEventArgs : EventArgs
    {
        public string SongId { get; }
        public string SongName { get; }
        public ulong GuildId { get; }
        public ulong TextChannelId { get; }

        internal ProcessedSongEventArgs(string songId, string songName, ulong guildId, ulong textChannelId)
        {
            SongId = songId;
            SongName = songName;
            GuildId = guildId;
            TextChannelId = textChannelId;
        }
    }
}
