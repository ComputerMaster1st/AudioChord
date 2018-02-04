using Shared.Music.Providers;
using Shared.Music.Providers.Storage;
using Shared.Music.Providers.Storage.FileSystem;
using Shared.Music.Providers.Storage.GridFS;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shared.Music
{
    public class MusicService
    {
        private MusicSettings settings;
        private IStorageProvider storageProvider;
        private MusicMetaDataProvider metaDataProvider;

        public IEnumerable<Playlist> PlayLists { get; private set; }

        public MusicService(MusicSettings settings)
        {
            this.settings = settings;

            switch (settings.Provider)
            {
                case StorageProvider.FileSystem:
                    storageProvider = new FileSystemProvider();
                    break;
                case StorageProvider.GridFS:
                    storageProvider = new GridFSProvider();
                    break;
                default:
                    throw new Exception("Unknown storage provider type");
            }

        }

        public async Task<Playlist> GetPlaylistAsync(Guid id)
        {
            return await storageProvider.GetPlaylistAsync(id);
        }

        public async Task<Song> GetSongAsync(Guid id)
        {
            SongMetadata data = metaDataProvider.GetSongMetadataAsync(id);

            return await Song.CreateExistingAsync(data, storageProvider);
        }
    }
}
