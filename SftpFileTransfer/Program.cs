using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SftpFileTransfer;

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
                services.AddScoped<SetTime, SetTime>();
                services.AddHostedService<SftpTransferService>();
                services.AddHostedService<SftpTransferReqFileService>();
                services.AddHostedService<SftpTransferNEFTReturnFileService>();
            })
            .Build()
            .Run();
    }
}
