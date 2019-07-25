using AWS.Logger;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Moneteer.Landing.V2
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddAWSProvider();

                    if (hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        logging.SetMinimumLevel(LogLevel.Debug);
                    } 
                    else 
                    {
                        logging.SetMinimumLevel(LogLevel.Information);
                    }
                })
                .UseStartup<Startup>();
        }
    }
}
