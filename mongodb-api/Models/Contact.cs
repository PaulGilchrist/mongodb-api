using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDbApi.Models {
    public class Contact {
        public Contact() {
            addresses = new HashSet<Address>();
            emails = new HashSet<Email>();
            phones = new HashSet<Phone>();
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string displayName { get; set; }
        public virtual ICollection<Address> addresses { get; set; }
        public virtual ICollection<Email> emails { get; set; }
        public virtual ICollection<Phone> phones { get; set; }
    }
}
