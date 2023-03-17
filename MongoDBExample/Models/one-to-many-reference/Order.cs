using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDBExample.Models.one_to_many_reference
{
    public class Order
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string OrderNumber { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> ItemIds { get; set; }
    }
}
