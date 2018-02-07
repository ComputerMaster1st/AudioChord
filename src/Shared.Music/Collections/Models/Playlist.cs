using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Shared.Music.Collections.Models
{
    public class Playlist
    {
        [BsonId] public Guid Id { get; internal set; } = new Guid();
        public List<Guid> SongList { get; internal set; } = new List<Guid>();
    }
}