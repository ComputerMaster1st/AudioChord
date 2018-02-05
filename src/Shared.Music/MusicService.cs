using MongoDB.Driver;

namespace Shared.Music
{
    public class MusicService
    {
        private SongCollection songCollection;

        public MusicService(MusicServiceConfig config)
        {
            MongoClient client = new MongoClient($"mongodb://{config.Username}:{config.Password}@localhost:27017/sharedmusic");
            IMongoDatabase database = client.GetDatabase("sharedmusic");

            songCollection = new SongCollection(database.GetCollection<SongMeta>(typeof(SongMeta).Name));
        }
    }
}
