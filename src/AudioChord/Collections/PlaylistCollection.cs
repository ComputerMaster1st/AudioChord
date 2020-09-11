using System;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AudioChord.Collections
{
    public class PlaylistCollection
    {
        private readonly IMongoCollection<Playlist> _collection;
        private SongCollection _songRepository;

        internal PlaylistCollection(IMongoDatabase database, SongCollection repository)
        {
            _collection = database.GetCollection<Playlist>(nameof(Playlist));
            _songRepository = repository;
        }

        /// <summary>
        /// Find a playlist in the database
        /// </summary>
        /// <param name="id">The id of the playlist to look for</param>
        /// <returns>A <see cref="Playlist"/> with songs</returns>
        /// <exception cref="ArgumentException">There was no <see cref="Playlist"/> with that id found in the database</exception>
        public async Task<Playlist> GetPlaylistAsync(ObjectId id)
        {
            return (await _collection.FindAsync(filter => filter.Id == id))
                   .FirstOrDefault() ??
                   throw new ArgumentException($"The Playlist id '{id}' was not found in the database");
        }

        /// <summary>
        /// Return all playlists in the database
        /// </summary>
        /// <returns>A list of all <see cref="Playlist"/></returns>
        public async Task<IEnumerable<Playlist>> GetAllAsync()
            => (await _collection.FindAsync(FilterDefinition<Playlist>.Empty)).ToEnumerable();


        /// <summary>
        /// Retrieve your playlist from database.
        /// </summary>
        /// <param name="playlist">Place playlist Id to fetch.</param>
        /// <returns>A <see cref="Playlist"/>Playlist contains list of all available song Ids.</returns>
        public Task UpdateAsync(Playlist playlist)
        {
            return _collection.ReplaceOneAsync(filter => filter.Id == playlist.Id, playlist,
                new UpdateOptions {IsUpsert = true});
        }

        /// <summary>
        /// Delete the playlist from database.
        /// </summary>
        /// <param name="id">The playlist Id to delete.</param>
        public Task DeleteAsync(ObjectId id)
        {
            return _collection.DeleteOneAsync(filter => filter.Id == id);
        }
    }
}