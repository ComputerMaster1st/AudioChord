using JetBrains.Annotations;

namespace AudioChord.Metadata.Postgres
{
    [PublicAPI]
    public static class PostgresMusicServiceBuilderExt
    {
        public static MusicServiceBuilder WithPostgresMetadataProvider(this MusicServiceBuilder builder, string connectionString)
        {
            builder.WithMetadataProvider(new PostgresMetadataProvider(new PostgresConnectionFactory(
                new ConnectionConfiguration()
                {
                    ConnectionString = connectionString
                })));

            return builder;
        }
    }
}