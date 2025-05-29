using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Renci.SshNet;
using Renci.SshNet.Sftp;

public class TransferReqFileScheduler
{
    private readonly string _configFile;
    private readonly int _intervalMinutes;

    public TransferReqFileScheduler(string configFile, int intervalMinutes)
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
                    if(job.IsSchedulerActiveForPayRequest)
                    {
                        ExecuteJob(job);
                    }
                    else
                    {
                        Console.WriteLine("Service is not start due to transfer-config.json like IsSchedulerActiveForPayRequest parameter is false and please update is true for start service");
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
        Console.WriteLine($"Checking directory: {job.TargetReqFileDirectory} on {job.TargetHost}");

        using (var sftpSource = new SftpClient(job.TargetHost, job.TargetPort, job.TargetUser, job.TargetPassword))
        {
            sftpSource.Connect();

            var files = sftpSource.ListDirectory(job.TargetReqFileDirectory)
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
                string tempFile = Path.GetTempFileName();
                try
                {
                    Console.WriteLine($"Downloading {file.Name}...");
                    using (var fs = File.Create(tempFile))
                    {
                        sftpSource.DownloadFile(file.FullName, fs);
                    }

                    using (var sftpTarget = new SftpClient(job.SourceHost, job.SourcePort, job.SourceUser, job.SourcePassword))
                    {
                        sftpTarget.Connect();
                        string targetPath = Path.Combine(job.RemoteReqFileDirectory, file.Name).Replace("\\", "/");
                        using (var fs = File.OpenRead(tempFile))
                        {
                            sftpTarget.UploadFile(fs, targetPath);
                        }
                        sftpTarget.Disconnect();
                    }

                    Console.WriteLine($"Transferred {file.Name} successfully.");

                    // Ensure DONE folder exists
                    string doneDirectory = Path.Combine(job.RemoteReqFileDirectory, "DONE").Replace("\\", "/");
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

            sftpSource.Disconnect();
        }
    }

}
