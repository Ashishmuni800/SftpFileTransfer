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
                return;
            }

            Console.WriteLine($"Found {files.Count} matching file(s).");

            foreach (var file in files)
            {
                if (!(file.Name.EndsWith(".TXT.SUCCESS", StringComparison.OrdinalIgnoreCase) ||
                      file.Name.EndsWith(".TXT.FAILURE", StringComparison.OrdinalIgnoreCase)))
                    continue;

                string tempFile = Path.GetTempFileName();

                try
                {
                    Console.WriteLine($"Processing file: {file.Name}");

                    using (var fs = File.Create(tempFile))
                    {
                        sftpSource.DownloadFile(file.FullName, fs);
                    }

                    using (var sftpTarget = new SftpClient(job.TargetHost, job.TargetPort, job.TargetUser, job.TargetPassword))
                    {
                        sftpTarget.Connect();

                        string targetPath = Path.Combine(job.TargetDirectory, file.Name).Replace("\\", "/");
                        string donePath = Path.Combine(job.TargetDirectoryDONE, file.Name).Replace("\\", "/");

                        // ✅ Check if the file already exists on target or DONE
                        bool alreadyExists = sftpTarget.Exists(targetPath) || sftpTarget.Exists(donePath);

                        if (alreadyExists)
                        {
                            Console.WriteLine($"File '{file.Name}' already exists on target — skipping transfer.");
                        }
                        else
                        {
                            Console.WriteLine($"Uploading '{file.Name}' to target...");
                            using (var fs = File.OpenRead(tempFile))
                            {
                                sftpTarget.UploadFile(fs, targetPath);
                            }
                            Console.WriteLine($"File '{file.Name}' transferred successfully.");
                        }

                        sftpTarget.Disconnect();
                    }

                    // ✅ Move to DONE folder on source
                    string doneDirectory = Path.Combine(job.RemoteDirectory, "DONE").Replace("\\", "/");
                    if (!sftpSource.Exists(doneDirectory))
                        sftpSource.CreateDirectory(doneDirectory);

                    string destinationPath = Path.Combine(doneDirectory, file.Name).Replace("\\", "/");
                    sftpSource.RenameFile(file.FullName, destinationPath);

                    Console.WriteLine($"Moved '{file.Name}' to DONE folder.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Failed to process {file.Name}: {ex.Message}");
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
