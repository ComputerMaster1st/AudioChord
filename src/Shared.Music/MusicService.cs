using MongoDB.Driver;

namespace Shared.Music
{
    public class MusicService
    {
        private IMongoDatabase database;

        public MusicService(MusicServiceConfig config)
        {
            MongoClient client = new MongoClient($"mongodb://{config.Username}:{config.Password}@localhost:27017/sharedmusic");
            database = client.GetDatabase("sharedmusic");
        }
    }
}
