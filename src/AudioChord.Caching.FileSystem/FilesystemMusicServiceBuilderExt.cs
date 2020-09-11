using JetBrains.Annotations;

namespace AudioChord.Caching.FileSystem
{
    [PublicAPI]
    public static class FilesystemMusicServiceBuilderExt
    {
        public static MusicServiceBuilder WithFilesystemCache(this MusicServiceBuilder builder, string cacheLocation)
        {
            builder.WithCache(new FileSystemCache(cacheLocation));
            return builder;
        }
    }
}