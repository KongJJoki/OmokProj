using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OmokGameServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<ServerOption>(hostContext.Configuration.GetSection("ServerOption"));
                    services.Configure<DBConfig>(hostContext.Configuration.GetSection("DBConfig"));
                    services.Configure<MatchingConfig>(hostContext.Configuration.GetSection("MatchingConfig"));
                    services.AddHostedService<MainServer>();
                })
                .Build();

            await host.RunAsync();
        }
    }
}