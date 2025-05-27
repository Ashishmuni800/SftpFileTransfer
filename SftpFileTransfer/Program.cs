using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

class Program
{
    static void Main(string[] args)
    {
        Host.CreateDefaultBuilder(args)
            .UseWindowsService()
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();    
                logging.AddConsole();         
                
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHostedService<SftpTransferService>();
            })
            .Build()
            .Run();
    }
}
