using System;
using System.Collections.Generic;

namespace AudioChord
{
    internal class ProcessSongRequestInfo
    {
        internal string VideoId { get; }
        internal Dictionary<ulong, Tuple<ulong, Playlist>> GuildsRequested { get; } = new Dictionary<ulong, Tuple<ulong, Playlist>>();

        internal ProcessSongRequestInfo(string videoId, ulong guildId, ulong textChannelId, Playlist playlist)
        {
            VideoId = videoId;
            GuildsRequested.Add(guildId, new Tuple<ulong, Playlist>(textChannelId, playlist));
        }
    }
}
