using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDBExample.Models;
using MongoDBExample.Models.many_to_many;
using MongoDBExample.Models.one_to_many_reference;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;

namespace MongoDBExample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IMongoCollection<Order> ordersCollection;
        private readonly IMongoCollection<Item> itemsCollection;
        public OrderController()
        {
            var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
            var dbName = Environment.GetEnvironmentVariable("DB_NAME");
            var connectionString = $"mongodb://{dbHost}:27017/{dbName}";

            var mongoUrl = MongoUrl.Create(connectionString);
            var mongoClient = new MongoClient(mongoUrl);
            var database = mongoClient.GetDatabase(mongoUrl.DatabaseName);
            ordersCollection = database.GetCollection<Order>("orders");
            itemsCollection = database.GetCollection<Item>("items");
            CheckAndAddDataToDB();
        }

        /// <summary>
        /// Aralarında one-to-many ilişki bulunan order ve product arasından sadece Order bilgisini çekiyoruz
        /// </summary>
        /// <returns></returns>
        [Route("GetAllOrders")]
        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            return Ok(await ordersCollection.Find(Builders<Order>.Filter.Empty).ToListAsync());
        }
        /// <summary>
        /// Aralarında one-to-many ilişki bulunan order ve product arasından sadece Product bilgisini çekiyoruz
        /// </summary>
        /// <returns></returns>

        [Route("GetAllProducts")]
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            return Ok(await itemsCollection.Find(Builders<Item>.Filter.Empty).ToListAsync());
        }
        /// <summary>
        /// Aralarında one-to-many ilişki bulunan order ve product tüm orderı ve bu ordera ait *productları birliklte çekiyoruz.
        /// </summary>
        /// <returns></returns>
        [Route("GetAllOrderWithProducts")]
        [HttpGet]
        public async Task<IActionResult> GetAllOrderWithProducts()
        {
            var result =await ordersCollection
                            .Aggregate()
                            .Lookup("items", "ItemIds", "_id", "Items")
                            .FirstOrDefaultAsync();
            return Ok(result.ToJson());
        }
        /// <summary>
        /// Aralarında one-to-many ilişki bulunan order ve product orderNumber parametresine ait orderı ve bu ordera ait *productları birliklte çekiyoruz.
        /// </summary>
        /// <returns></returns>
        [Route("GetAllOrderWithProductsByOrderNumber")]
        [HttpGet]
        public async Task<IActionResult> GetAllOrderWithProductsByOrderNumber(string orderNumber)
        {
            var result = await ordersCollection.Aggregate()
                                          .Match(o => o.OrderNumber == orderNumber)
                                          .Lookup("items", "ItemIds", "_id", "Items")
                                          .FirstOrDefaultAsync();
            return Ok(result.ToJson());
        }

        private void CheckAndAddDataToDB()
        {
            var data = ordersCollection.Find(Builders<Order>.Filter.Empty).ToList();
            var data2 = itemsCollection.Find(Builders<Item>.Filter.Empty).ToList();
            if (data.Count == 0)
            {
                // Create two order and two product
                var order = new Order { OrderNumber="123", ItemIds  = new List<string>() };
                var item1 = new Item { ItemName="Saat", Price=50 };
                var item2 = new Item { ItemName ="Telefon", Price =86 };

                // Insert the order and product into their respective collections
                ordersCollection.InsertMany(new[] { order });
                itemsCollection.InsertMany(new[] { item1, item2 });

                // Add the product IDs to the order list of each product
                order.ItemIds.Add(item1.Id);
                order.ItemIds.Add(item2.Id);

                // Update the orders collections with the new data
                ordersCollection.ReplaceOne(Builders<Order>.Filter.Eq(u => u.Id, order.Id), order);


            }
        }
    }
}
