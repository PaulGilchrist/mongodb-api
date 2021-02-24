using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDbApi.Models {
    public class Contact {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string displayName { get; set; }
        public Address[] addresses { get; set; }
        public Email[] emails { get; set; }
        public Phone[] phones { get; set; }
    }
}
