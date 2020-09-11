using System.Threading.Tasks;
using Npgsql;

namespace AudioChord.Metadata.Postgres
{
    public class PostgresConnectionFactory
    {
        private readonly ConnectionConfiguration _configuration;
        
        public PostgresConnectionFactory(ConnectionConfiguration configuration)
        {
            _configuration = configuration;
        }
        
        public async Task<NpgsqlConnection> CreateOpenConnectionAsync()
        {
            NpgsqlConnection connection = new NpgsqlConnection(_configuration.ConnectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}