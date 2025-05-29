using Microsoft.Extensions.Hosting;
using SftpFileTransfer;
using System;
using System.Threading;
using System.Threading.Tasks;

public class SftpTransferReqFileService : BackgroundService
{
    private readonly SetTime _setTime;
    public SftpTransferReqFileService(SetTime setTime)
    {
        _setTime = setTime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var scheduler = new TransferReqFileScheduler("transfer-config.json", _setTime.GetTimeforTransferReqFileScheduler());

        await Task.Run(() => scheduler.Start(), stoppingToken);
    }
}
