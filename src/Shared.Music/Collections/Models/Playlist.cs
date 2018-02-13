using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shared.Music.Collections.Models
{
    public class Playlist
    {
        //the id of the playlist in the database
        internal ObjectId Id { get; private set; } = new ObjectId();
        public List<MusicMeta> Songs { get; private set; } = new List<MusicMeta>();

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