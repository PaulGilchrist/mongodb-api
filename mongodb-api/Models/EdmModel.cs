using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;

namespace MongoDbApi.Models {
    public static class AppEdmModel {
        public static IEdmModel GetModel() {
            var builder = new ODataConventionModelBuilder();
            // Expand not yet supported, and Select only supported with GetById
            builder.EntitySet<Contact>("contacts").EntityType.Count().Filter().OrderBy().Page(null, 100).Select();
            return builder.GetEdmModel();
        }
    }
}
