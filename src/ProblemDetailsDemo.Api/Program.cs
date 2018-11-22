using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace ProblemDetailsDemo.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
//                .UseEnvironment(EnvironmentName.Production) // Uncomment to remove exception details from responses.
                .UseStartup<Startup>();
        }
    }
}