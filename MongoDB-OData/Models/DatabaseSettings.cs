namespace MongoDB_OData.Models {
    public class DatabaseSettings: IDatabaseSettings {
        public string ContactsCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface IDatabaseSettings {
        string ContactsCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
