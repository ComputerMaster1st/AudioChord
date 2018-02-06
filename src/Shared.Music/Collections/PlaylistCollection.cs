using MongoDB.Driver;
using Shared.Music.Collections.Models;
using System;
using System.Threading.Tasks;

namespace Shared.Music.Collections
{
    internal class PlaylistCollection
    {
        private IMongoCollection<Playlist> collection;

        internal PlaylistCollection(IMongoCollection<Playlist> collection)
        {
            this.collection = collection;
        }

        private Playlist CreatePlaylist(Guid playlistId)
        {
            return new Playlist() { Id = playlistId };
        }

        public async Task<bool> UpdatePlaylistAsync(Guid songId, Guid playlistId)
        {
            var result = await collection.FindAsync((f) => f.Id.Equals(playlistId));
            Playlist playlist = await result.FirstOrDefaultAsync();

            if (playlist == null) return false;
            else if (playlist.Contains(songId)) return false;

            playlist.Add(songId);
            await collection.ReplaceOneAsync((f) => f.Id.Equals(playlistId), playlist, new UpdateOptions() { IsUpsert = true });
            return true;
        }
    }
}
