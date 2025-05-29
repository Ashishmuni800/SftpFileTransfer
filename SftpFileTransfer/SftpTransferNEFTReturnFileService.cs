using Microsoft.Extensions.Hosting;
using SftpFileTransfer;
using System;
using System.Threading;
using System.Threading.Tasks;

public class SftpTransferNEFTReturnFileService : BackgroundService
{
    private readonly SetTime _setTime;
    public SftpTransferNEFTReturnFileService(SetTime setTime)
    {
        _setTime = setTime;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var scheduler = new TransferNEFTReturnFileScheduler("transfer-config.json", _setTime.GetTimeforTransferNEFTReturnFileScheduler());

        await Task.Run(() => scheduler.Start(), stoppingToken);
    }
}
