using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDBExample.Models.one_ro_huge;
using MongoDBExample.Models.one_to_many_embedded;
using MongoDBExample.Models.one_to_many_reference;

namespace MongoDBExample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HostLogController : ControllerBase
    {
        private readonly IMongoCollection<HostInfo> hostInfoCollection;
        private readonly IMongoCollection<Log> logCollection;
        public HostLogController()
        {
            var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
            var dbName = Environment.GetEnvironmentVariable("DB_NAME");
            var connectionString = $"mongodb://{dbHost}:27017/{dbName}";

            var mongoUrl = MongoUrl.Create(connectionString);
            var mongoClient = new MongoClient(mongoUrl);
            var database = mongoClient.GetDatabase(mongoUrl.DatabaseName);
            hostInfoCollection = database.GetCollection<HostInfo>("hosts");
            logCollection = database.GetCollection<Log>("logs");
            CheckAndAddDataToDB();
        }
        [HttpGet]
        public async Task<IActionResult> GetAllHostLog()
        {

            var res = await Task.Run(() =>
            {
                var query = from host in hostInfoCollection.AsQueryable()
                            join log in logCollection.AsQueryable() on host.Id equals log.Host into hostLogs
                            select new
                            {
                                HostId = host.Id,
                                HostName = host.Name,
                                HostLogs = hostLogs
                            };
                return query.ToList();
            });
            return Ok(res);
        }
        private void CheckAndAddDataToDB()
        {
            var data = hostInfoCollection.Find(Builders<HostInfo>.Filter.Empty).ToList();
            if (data.Count == 0)
            {
                var host = new HostInfo { Name = "gofy.example.com", IpAddress = "127.66.48.48" };
                var log1 = new Log { Message = "cpu is on fire" };
                var log2 = new Log { Message = "memory full" };

                hostInfoCollection.InsertMany(new[] { host });
                logCollection.InsertMany(new[] { log1, log2 });

                log1.Host = host.Id;
                log2.Host = host.Id;

                logCollection.ReplaceOne(Builders<Log>.Filter.Eq(u => u.Id, log1.Id), log1);
                logCollection.ReplaceOne(Builders<Log>.Filter.Eq(u => u.Id, log2.Id), log2);
            }
        }
    }
}
