using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shared.Music.Collections.Models
{
    public class Playlist
    {
        //the id of the playlist in the database
        [BsonId] internal ObjectId Id { get; private set; } = ObjectId.GenerateNewId();
        public List<Song> Songs { get; private set; } = new List<Song>();

        private readonly PlaylistCollection playlistStorage;

        internal Playlist(PlaylistCollection storage)
        {
            playlistStorage = storage;
        }

        public async Task Save()
        {
            await playlistStorage.UpdateAsync(this);
        }
    }
}