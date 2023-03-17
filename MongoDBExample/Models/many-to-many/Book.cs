using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDBExample.Models.many_to_many
{
    public class Book
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Title { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> ReaderIds { get; set; }
    }
}
