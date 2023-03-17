using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace MongoDBExample.Models.one_ro_huge
{
    public class HostInfo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
    }
}
