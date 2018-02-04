using Shared.Music.Providers.Storage;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Shared.Music
{
    public class Song
    {
        public string Name { get; private set; }
        public TimeSpan Length { get; private set; }
        public Guid Id { get; private set; }

        public string Uploader { get; private set; }

        public Stream MusicStream { get; private set; }

        internal async static Task<Song> CreateExistingAsync(SongMetadata metadata, IStorageProvider storageProvider)
        {
            return new Song(metadata.id,
                metadata.name,
                metadata.length,
                metadata.uploader,
                (await storageProvider.GetSongAsync(metadata.id))
            );
        }

        private Song(Guid id, string name, TimeSpan length, string uploader, Stream music)
        {
            Name = name;
            Length = length;
            Uploader = uploader;

            MusicStream = music;

            Id = id;
        }

        internal Song(string name, TimeSpan length, string uploader, Stream music)
        {
            Name = name;
            Length = length;
            Uploader = uploader;

            MusicStream = music;

            Id = Guid.NewGuid();
        }

    }
}
