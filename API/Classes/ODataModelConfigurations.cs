using API.Models;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNetCore.Mvc;

namespace API.Classes {
    /// <summary>
    /// Represents the model configuration for all configurations.
    /// </summary>
    public class ODataModelConfigurations : IModelConfiguration {
        /// <summary>
        /// Applies model configurations using the provided builder for the specified API version.
        /// </summary>
        /// <param name="builder">The <see cref="ODataModelBuilder">builder</see> used to apply configurations.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the <paramref name="builder"/>.</param>
        public void Apply(ODataModelBuilder builder, ApiVersion apiVersion) {
            builder.Namespace = "ApiTemplate";
            builder.ContainerName = "ApiTemplateContainer";
            // Called once for each apiVersion, so this is the best place to define the EntiitySet differences from version to version
            // IMPORTANT - The entity set name is case sensitive and must match the controller name or the POST properties will not show as coming [FromBody] but rather from the (query) in Swagger
            var contact = builder.EntitySet<Contact>("Contacts").EntityType;
            contact.Count().Expand().Filter().Count().Page(100, null).OrderBy().Select(); // .Expand()
        }
    }
}