using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

public class SftpTransferService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var scheduler = new TransferScheduler("transfer-config.json", 1);

        await Task.Run(() => scheduler.Start(), stoppingToken);
    }
}
