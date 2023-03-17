using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDBExample.Models;
using MongoDBExample.Models.many_to_many;

namespace MongoDBExample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserBookController : ControllerBase
    {
        private readonly IMongoCollection<User> usersCollection;
        private readonly IMongoCollection<Book> booksCollection;
        public UserBookController()
        {
            var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
            var dbName = Environment.GetEnvironmentVariable("DB_NAME");
            var connectionString = $"mongodb://{dbHost}:27017/{dbName}";

            var mongoUrl = MongoUrl.Create(connectionString);
            var mongoClient = new MongoClient(mongoUrl);
            var database = mongoClient.GetDatabase(mongoUrl.DatabaseName);
            usersCollection = database.GetCollection<User>("users");
            booksCollection = database.GetCollection<Book>("books");
            CheckAndAddDataToDB();
        }
        /// <summary>
        /// Arların many-to-many ilişki bulunan User ve book için "John" isimli kullanıcıya ait 
        /// user bilgilerini ve bookları listeliyor.
        /// </summary>
        /// <returns></returns>
        [Route("GetSpecificUserBook")]
        [HttpGet]
        public async Task<IActionResult> GetSpecificBook(string userName)
        {
            // Create an aggregation pipeline to retrieve users and their favorite books
            var pipeline = new BsonDocument[]
            {
              // Match documents in the users collection where the name is "John"
              new BsonDocument("$match", new BsonDocument("Name", "John")),
              
              // Lookup the books collection using the FavoriteBookIds field
              new BsonDocument("$lookup", new BsonDocument
              {
                  { "from", "books" },
                  { "localField", "FavoriteBookIds" },
                  { "foreignField", "_id" },
                  { "as", "FavoriteBooks" }
              })
            };

            // Execute the aggregation pipeline and get the results
            var results = usersCollection.Aggregate<BsonDocument>(pipeline).ToList();
            return Ok(results.ToJson());
        }
        /// <summary>
        /// Arların many-to-many ilişki bulunan User ve book için tüm user ve userlara ait book bilgilerini döndürüyor.
        /// </summary>
        /// <returns></returns>
        [Route("GetAllUserBook")]
        [HttpGet]
        public async Task<IActionResult> GetAllUserBook()
        {
            // Create an aggregation pipeline to retrieve users and their favorite books
            var pipeline = new BsonDocument[]
            {             
              // Lookup the books collection using the FavoriteBookIds field
              new BsonDocument("$lookup", new BsonDocument
              {
                  { "from", "books" },
                  { "localField", "FavoriteBookIds" },
                  { "foreignField", "_id" },
                  { "as", "FavoriteBooks" }
              })
            };

            // Execute the aggregation pipeline and get the results
            var results = await usersCollection.Aggregate<BsonDocument>(pipeline).ToListAsync();
            return Ok(results.ToJson());
        }


        private void CheckAndAddDataToDB()
        {
            var data = usersCollection.Find(Builders<User>.Filter.Empty).ToList();
            if(data.Count == 0)
            {
                // Create two users and two books
                var user1 = new User { Name = "John", FavoriteBookIds = new List<string>() };
                var user2 = new User { Name = "Sarah", FavoriteBookIds = new List<string>() };
                var book1 = new Book { Title = "Pride and Prejudice", ReaderIds = new List<string>() };
                var book2 = new Book { Title = "To Kill a Mockingbird", ReaderIds = new List<string>() };

                // Insert the users and books into their respective collections
                usersCollection.InsertMany(new[] { user1, user2 });
                booksCollection.InsertMany(new[] { book1, book2 });

                // Add the book IDs to the favorite book list of each user
                user1.FavoriteBookIds.Add(book1.Id);
                user1.FavoriteBookIds.Add(book2.Id);
                user2.FavoriteBookIds.Add(book2.Id);

                // Add the user IDs to the reader list of each book
                book1.ReaderIds.Add(user1.Id);
                book2.ReaderIds.Add(user1.Id);
                book2.ReaderIds.Add(user2.Id);

                // Update the users and books collections with the new data
                usersCollection.ReplaceOne(Builders<User>.Filter.Eq(u => u.Id, user1.Id), user1);
                usersCollection.ReplaceOne(Builders<User>.Filter.Eq(u => u.Id, user2.Id), user2);
                booksCollection.ReplaceOne(Builders<Book>.Filter.Eq(b => b.Id, book1.Id), book1);
                booksCollection.ReplaceOne(Builders<Book>.Filter.Eq(b => b.Id, book2.Id), book2);
            }
        }
    }
}
