using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace ODataCoreTemplate {
    public static class Program {
        public static async Task Main(string[] args) {
            var webHost = CreateWebHostBuilder(args).Build();
            await webHost.RunAsync();
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => {
                webBuilder.ConfigureKestrel(serverOptions => {
                    // Set properties and call methods on options
                    serverOptions.Limits.MaxRequestLineSize = 65536;
                })
                .UseStartup<Startup>();
            });

    }
}
