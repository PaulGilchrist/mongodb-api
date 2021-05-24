using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using mongodbapi.Classes;
using MongoDbApi.Models;
using MongoDbApi.Services;

namespace MongoDbApi {
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
            //services.AddControllers();
            services.AddOData();
            services.AddRouting();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,IWebHostEnvironment env) {
            if(env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors("AllOrigins");
            app.UseHttpsRedirection();
            app.UseRouting();
            // app.UseAuthorization();
            app.UseEndpoints(endpoints => {
                // endpoints.MapControllers();
                endpoints.Expand().Filter().Count().OrderBy();
                endpoints.MapODataRoute("odata","odata",AppEdmModel.GetModel());
            });
        }
    }
}
