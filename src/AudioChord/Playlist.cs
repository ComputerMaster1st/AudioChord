using AudioChord.Collections.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace AudioChord
{
    public class Playlist : PlaylistStub
    {
        [BsonIgnore]
        public List<ISong> Songs { get; private set; }

        internal Playlist(ObjectId id, List<ISong> songs) : base(id)
            => Songs = songs;

        public Playlist() : this(ObjectId.GenerateNewId(), new List<ISong>())
        { }

        internal PlaylistStub ConvertToDatabaseRepresentation()
        {
            SongIds = Songs.ConvertAll(new Converter<ISong, SongId>((song) => { return song.Id; }));
            return this;
        }
    }
}