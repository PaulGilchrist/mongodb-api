using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using API.Classes;
using API.Models;
using API.Services;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace ODataCoreTemplate {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            // CORS support
            services.AddCors(options => {
                options.AddPolicy("AllOrigins",
                     builder => {
                         builder
                     .AllowAnyOrigin()
                     .AllowAnyMethod()
                     .AllowAnyHeader();
                     });
            });
            services.AddSingleton<ApplicationSettings>();
            services.AddSingleton<ContactService>();
            services.AddSingleton<Logger>();
            services.AddOptions();
            services.AddMvc(options => options.EnableEndpointRouting = false).SetCompatibilityVersion(CompatibilityVersion.Latest)
                .AddNewtonsoftJson(options => {
                    options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                    options.SerializerSettings.Formatting = Formatting.None;
                    options.SerializerSettings.PreserveReferencesHandling=PreserveReferencesHandling.None;
                    options.SerializerSettings.NullValueHandling=NullValueHandling.Ignore;
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });
            services.AddApiVersioning(options => {
                //options.ApiVersionReader = new QueryStringApiVersionReader();
                options.ReportApiVersions = true;
                // required when adding versioning to and existing API to allow existing non-versioned queries to succeed (not error with no version specified)
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = ApiVersions.V1;
                // Needed to fix issue #1754 - https://github.com/OData/WebApi/issues/1754
                options.RegisterMiddleware = false;
            });
            services.AddOData().EnableApiVersioning();
            services.AddODataApiExplorer(options => {
                // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                // note: the specified format code will format the version as "'v'major[.minor][-status]"
                options.GroupNameFormat = "'v'VVV";
            });
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(options => {
                options.IncludeXmlComments(XmlCommentsFilePath);
                //Configure Swagger to filter out $expand objects to improve performance for large highly relational APIs
                options.SchemaFilter<SwaggerIgnoreFilter>();
                options.OperationFilter<SwaggerDefaultValues>();
            });
            // The IHttpContextAccessor service is not registered by default.  The clientId/clientIp resolvers use it.  https://github.com/aspnet/Hosting/issues/793
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            // Add support for GetUrlHelper used in ReferenceHelper class
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddScoped<IUrlHelper>(x => {
                var actionContext = x.GetRequiredService<IActionContextAccessor>().ActionContext;
                var factory = x.GetRequiredService<IUrlHelperFactory>();
                return factory.GetUrlHelper(actionContext);
            });
            services.AddSwaggerGenNewtonsoftSupport();
        }

        /// <summary>
        /// Configures the application using the provided builder, hosting environment, and logging factory.
        /// </summary>
        /// <param name="app">The current application builder.</param>
        /// <param name="env">The current hosting environment.</param>
        /// <param name="httpContextAccessor">Allows access to the HTTP context including request/response</param>
        /// <param name="modelBuilder">The <see cref="VersionedODataModelBuilder">model builder</see> used to create OData entity data models (EDMs).</param>
        /// <param name="provider">The API version descriptor provider used to enumerate defined API versions.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor, VersionedODataModelBuilder modelBuilder, IApiVersionDescriptionProvider provider) {
            var httpRequestLoggingLevel = Configuration.GetValue<string>("ApplicationInsights:HttpRequestLoggingLevel");
            app.UseODataBatching();
            app.UseApiVersioning(); // added to fix issue outlined in https://github.com/OData/WebApi/issues/1754
            app.UseMvc(routes => {
                routes.Count().Filter().OrderBy().Expand().Select().MaxTop(null);
                routes.MapVersionedODataRoutes("ODataRoute", "", modelBuilder.GetEdmModels());
                routes.MapODataServiceRoute("ODataBatch", null,
                   configureAction: containerBuilder => containerBuilder
                       .AddService(Microsoft.OData.ServiceLifetime.Singleton, typeof(IEdmModel),
                           sp => modelBuilder.GetEdmModels().First())
                       .AddService(Microsoft.OData.ServiceLifetime.Singleton, typeof(IEnumerable<IODataRoutingConvention>),
                           sp => ODataRoutingConventions.CreateDefaultWithAttributeRouting("ODataBatch", routes))
                        .AddService(Microsoft.OData.ServiceLifetime.Singleton, typeof(ODataMessageReaderSettings),
                            sp => {
                                ODataMessageReaderSettings odataMessageReaderSettings = new ODataMessageReaderSettings();
                                odataMessageReaderSettings.MessageQuotas.MaxOperationsPerChangeset = 5000;
                                return odataMessageReaderSettings;
                            })
                );
                routes.EnableDependencyInjection();
            });
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(options => {
                foreach (var description in provider.ApiVersionDescriptions) {
                    options.SwaggerEndpoint(
                        $"/swagger/{description.GroupName}/swagger.json",
                        description.GroupName.ToUpperInvariant());
                }
                options.DefaultModelExpandDepth(2);
                options.DefaultModelsExpandDepth(-1);
                options.DefaultModelRendering(ModelRendering.Model);
                options.DisplayRequestDuration();
                //options.DocExpansion(DocExpansion.None);
            });
        }

        static string XmlCommentsFilePath {
            get {
                var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                var fileName = typeof(Startup).GetTypeInfo().Assembly.GetName().Name + ".xml";
                return Path.Combine(basePath, fileName);
            }
        }


    }
}
