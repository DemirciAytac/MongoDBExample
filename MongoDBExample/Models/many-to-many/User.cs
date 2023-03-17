using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDBExample.Models.many_to_many
{
    // Define the classes that will represent the documents in the two collections
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public List<string> FavoriteBookIds { get; set; }
    }
}
