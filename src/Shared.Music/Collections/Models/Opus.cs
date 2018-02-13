using System;
using System.IO;
using MongoDB.Bson;

namespace Shared.Music.Collections.Models
{
    public class Opus : Song
    {
        public Opus(string Name, TimeSpan Length, string Uploader, ObjectId OpusId) : base(Name, Length, Uploader, OpusId)
        {
        }

        public Stream OpusStream { get; internal set; }
    }
}
