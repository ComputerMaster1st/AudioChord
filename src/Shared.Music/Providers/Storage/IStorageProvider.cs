using System;
using System.IO;
using System.Threading.Tasks;

namespace Shared.Music.Providers.Storage
{
    internal interface IStorageProvider
    {
        Task<Stream> GetSongAsync(Guid id);
        Task<Playlist> GetPlaylistAsync(Guid id);
    }
}
