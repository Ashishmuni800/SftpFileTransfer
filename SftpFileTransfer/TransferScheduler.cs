using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Renci.SshNet;
using Renci.SshNet.Sftp;

public class TransferScheduler
{
    private readonly string _configFile;
    private readonly int _intervalMinutes;

    public TransferScheduler(string configFile, int intervalMinutes)
    {
        _configFile = configFile;
        _intervalMinutes = intervalMinutes;
    }

    public void Start()
    {
        Console.WriteLine($"Scheduler started. Running every {_intervalMinutes} minute(s).");
        while (true)
        {
            try
            {
                var jobs = LoadJobs();
                foreach (var job in jobs)
                {
                    if (job.IsSchedulerActiveForPayResponse)
                    {
                        ExecuteJob(job);
                    }
                    else
                    {
                        Console.WriteLine("The service is not starting because the IsSchedulerActiveForPayResponse parameter in transfer-config.json is set to false. Please update it to true to start the service.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Scheduler exception: {ex.Message}");
            }

            Thread.Sleep(TimeSpan.FromMinutes(_intervalMinutes));
        }
    }

    private List<TransferJob> LoadJobs()
    {
        var json = File.ReadAllText(_configFile);
        return JsonSerializer.Deserialize<List<TransferJob>>(json);
    }

    private void ExecuteJob(TransferJob job)
    {
        Console.WriteLine($"Checking directory: {job.RemoteDirectory} on {job.SourceHost}");

        using (var sftpSource = new SftpClient(job.SourceHost, job.SourcePort, job.SourceUser, job.SourcePassword))
        {
            sftpSource.Connect();

            var files = sftpSource.ListDirectory(job.RemoteDirectory)
                .Where(f => !f.IsDirectory && job.FilenamePrefixes.Any(prefix => f.Name.StartsWith(prefix)))
                .ToList();

            if (!files.Any())
            {
                Console.WriteLine("No matching files found.");
                sftpSource.Disconnect();
                return;
            }

            Console.WriteLine($"Found {files.Count} matching file(s).");

            foreach (var file in files)
            {
                if ((file.Name.EndsWith(".TXT.SUCCESS", StringComparison.OrdinalIgnoreCase) ||
                   file.Name.EndsWith(".TXT.FAILURE", StringComparison.OrdinalIgnoreCase)))
                {
                    string tempFile = Path.GetTempFileName();
                    try
                    {
                        Console.WriteLine($"Downloading {file.Name}...");
                        using (var fs = File.Create(tempFile))
                        {
                            sftpSource.DownloadFile(file.FullName, fs);
                        }

                        using (var sftpTarget = new SftpClient(job.TargetHost, job.TargetPort, job.TargetUser, job.TargetPassword))
                        {
                            sftpTarget.Connect();
                            string targetPath = Path.Combine(job.TargetDirectory, file.Name).Replace("\\", "/");
                            using (var fs = File.OpenRead(tempFile))
                            {
                                
                                //var ExistPath = "/home/pnbdata/BKSY/JAP_IT/MGNR/REPORTS/DONE";
                                //var ExistPath2 = "/home/pnbdata/BKSY/JAP_IT/MGNR/REPORTS";
                                var files2 = sftpSource.ListDirectory(job.TargetDirectoryDONE)
                                .Where(f => !f.IsDirectory && job.FilenamePrefixes.Any(prefix => f.Name.StartsWith(prefix)))
                                .ToList();
                                var files3 = sftpSource.ListDirectory(job.TargetDirectory)
                                .Where(f => !f.IsDirectory && job.FilenamePrefixes.Any(prefix => f.Name.StartsWith(prefix)))
                                .ToList();
                                foreach (var item in files2)
                                {
                                    foreach (var item1 in files3)
                                    {
                                        if (item.FullName != file.FullName && item1.FullName !=file.FullName)
                                        {
                                            sftpTarget.UploadFile(fs, targetPath);
                                        }
                                    }
                                }
                            }
                            sftpTarget.Disconnect();
                        }

                        Console.WriteLine($"Transferred {file.Name} successfully.");

                        // Ensure DONE folder exists
                        string doneDirectory = Path.Combine(job.RemoteDirectory, "DONE").Replace("\\", "/");
                        if (!sftpSource.Exists(doneDirectory))
                        {
                            sftpSource.CreateDirectory(doneDirectory);
                        }

                        // Move file to DONE folder
                        string sourcePath = file.FullName;
                        string destinationPath = Path.Combine(doneDirectory, file.Name).Replace("\\", "/");
                        sftpSource.RenameFile(sourcePath, destinationPath);

                        Console.WriteLine($"Moved {file.Name} to DONE folder.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] Failed to transfer {file.Name}: {ex.Message}");
                    }
                    finally
                    {
                        if (File.Exists(tempFile))
                            File.Delete(tempFile);
                    }
                }
                else
                {
                    continue;
                }
            }

            sftpSource.Disconnect();
        }
    }

}
