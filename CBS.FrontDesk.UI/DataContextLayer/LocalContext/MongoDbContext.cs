using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContextLayer.LocalContext
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MongoDBConnection"].ConnectionString;
            var mongoClient = new MongoClient(connectionString);
            _database = mongoClient.GetDatabase("TSCUIDB");
        }

        public IMongoCollection<RequestLog> RequestLogs => _database.GetCollection<RequestLog>("RequestLogs");
    }
}
