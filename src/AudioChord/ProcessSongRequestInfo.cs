using System;
using System.Collections.Generic;

namespace AudioChord
{
    internal class ProcessSongRequestInfo
    {
        internal string VideoId { get; }
        internal string VideoUrl { get; }

        internal Dictionary<ulong, Tuple<ulong, Playlist>> GuildsRequested { get; } = new Dictionary<ulong, Tuple<ulong, Playlist>>();

        internal ProcessSongRequestInfo(string videoId, string videoUrl, ulong guildId, ulong textChannelId, Playlist playlist)
        {
            VideoId = videoId;
            VideoUrl = videoUrl;
            GuildsRequested.Add(guildId, new Tuple<ulong, Playlist>(textChannelId, playlist));
        }
    }
}