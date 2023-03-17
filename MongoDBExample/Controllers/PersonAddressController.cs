using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDBExample.Models.many_to_many;
using MongoDBExample.Models.one_to_many_embedded;
using MongoDBExample.Models.one_to_many_reference;

namespace MongoDBExample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonAddressController : ControllerBase
    {
        private readonly IMongoCollection<Person> personCollection;
        public PersonAddressController()
        {
            var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
            var dbName = Environment.GetEnvironmentVariable("DB_NAME");
            var connectionString = $"mongodb://{dbHost}:27017/{dbName}";

            var mongoUrl = MongoUrl.Create(connectionString);
            var mongoClient = new MongoClient(mongoUrl);
            var database = mongoClient.GetDatabase(mongoUrl.DatabaseName);
            personCollection = database.GetCollection<Person>("persons");
            CheckAndAddDataToDB();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPerson()
        {
            var res = personCollection.Find(Builders<Person>.Filter.Empty).ToList();
            return Ok(res);
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetPerson(string name)
        {
            var person = await personCollection.Find(x => x.Name == name).FirstOrDefaultAsync();
            return Ok(person);
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePerson(Person person)
        {
            personCollection.ReplaceOne(u => u.Id == person.Id, person);
            return Ok(person);
        }

        private void CheckAndAddDataToDB()
        {
            var data = personCollection.Find(Builders<Person>.Filter.Empty).ToList();
            if(data.Count == 0 )
            {
                var address1 = new Address { Street = "45 Sesame st.", City = "Los Angeles" };
                var address2 = new Address { Street = "123 Avenue st.", City = "New York" };
                var person = new Person { Name = "Aytaç", Company = "MongoDB",Addresses = new List<Address> { address1,address2} };

                personCollection.InsertOne(person);
            }
        }
    }
}
