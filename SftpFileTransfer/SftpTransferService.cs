﻿using Microsoft.Extensions.Hosting;
using SftpFileTransfer;
using System;
using System.Threading;
using System.Threading.Tasks;

public class SftpTransferService : BackgroundService
{
    private readonly SetTime _setTime;
    public SftpTransferService(SetTime setTime)
    {
        _setTime = setTime;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var scheduler = new TransferScheduler("transfer-config.json", _setTime.GetTimeforTransferScheduler());

        await Task.Run(() => scheduler.Start(), stoppingToken);
    }
}
