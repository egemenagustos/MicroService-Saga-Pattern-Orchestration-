using MongoDB.Driver;

namespace Stock.API.Services
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _mongoDatabase;

        public MongoDbService(IConfiguration configuration)
        {
            MongoClient mongoClient = new MongoClient(configuration["MongoDb"]);
            _mongoDatabase = mongoClient.GetDatabase("StockDb");
        }

        public IMongoCollection<T> GetCollection<T>()
            => _mongoDatabase.GetCollection<T>(typeof(T).Name.ToLowerInvariant());
    }
}
