using MongoDB.Bson;
using System.IO;
using System.Threading.Tasks;

namespace AudioChord.Collections.Models
{
    public class Song
    {
        public string Id { get; internal set; }
        public SongMetadata Metadata { get; internal set; }

        private readonly SongCollection songCollection;
        internal ObjectId opusFileId;

        internal Song(string id, SongMetadata metadata, ObjectId opusId, SongCollection collection)
        {
            Id = id;
            Metadata = metadata;

            opusFileId = opusId;
            songCollection = collection;
        }

        public async Task<Stream> GetMusicStreamAsync()
        {
            return await songCollection.OpenOpusStreamAsync(this);
        }
    }
}
