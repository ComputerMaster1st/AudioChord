namespace AudioChord
{
    public class SongsExistedEventArgs
    {
        public ulong GuildId { get; }
        public ulong TextChannelId { get; }
        public int InstalledExistingSongsCount { get; }
        public int QueuedSongsCount { get; }

        internal SongsExistedEventArgs(ulong guildId, ulong textChannelId, int installedExistingSongsCount, int queuedSongsCount)
        {
            GuildId = guildId;
            TextChannelId = textChannelId;
            InstalledExistingSongsCount = installedExistingSongsCount;
            QueuedSongsCount = queuedSongsCount;
        }
    }
}