using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace MongoDBExample.Models.one_ro_huge
{
    public class Log
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Message { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string Host { get; set; }
    }
}
