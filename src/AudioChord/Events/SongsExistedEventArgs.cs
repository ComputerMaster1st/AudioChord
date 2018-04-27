using System.Collections.Generic;

namespace AudioChord
{
    public class SongsExistedEventArgs
    {
        public ulong GuildId { get; }
        public ulong TextChannelId { get; }
        public IEnumerable<string> AlreadyExistingSongs { get; }

        internal SongsExistedEventArgs(ulong guildId, ulong textChannelId, IEnumerable<string> alreadyExistingSongs)
        {
            GuildId = guildId;
            TextChannelId = textChannelId;
            AlreadyExistingSongs = alreadyExistingSongs;
        }
    }
}