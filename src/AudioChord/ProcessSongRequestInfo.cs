using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace AudioChord
{
    internal class ProcessSongRequestInfo
    {
        internal string VideoId { get; }
        internal Dictionary<ulong, Tuple<ulong, ObjectId>> GuildsRequested { get; } = new Dictionary<ulong, Tuple<ulong, ObjectId>>();

        internal ProcessSongRequestInfo(string videoId, ulong guildId, ulong textChannelId, ObjectId playlistId)
        {
            VideoId = videoId;
            GuildsRequested.Add(guildId, new Tuple<ulong, ObjectId>(textChannelId, playlistId));
        }
    }
}
