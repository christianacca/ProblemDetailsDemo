using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ProblemDetailsDemo.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
          .ConfigureWebHostDefaults(webBuilder => webBuilder
            .UseStartup<Startup>()
            // .UseEnvironment(Environments.Production) // Uncomment to remove exception details from responses.
          );
    }
}