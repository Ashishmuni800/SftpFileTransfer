using System.Collections.Generic;

public class TransferJob
{
    public string SourceHost { get; set; }
    public int SourcePort { get; set; } = 22;
    public string SourceUser { get; set; }
    public string SourcePassword { get; set; }

    public string RemoteDirectory { get; set; } // e.g., "/remote/path/"
    public List<string> FilenamePrefixes { get; set; } // ["N5132Bk", "T5132Bk"]

    public string TargetHost { get; set; }
    public int TargetPort { get; set; } = 22;
    public string TargetUser { get; set; }
    public string TargetPassword { get; set; }
    public string TargetDirectory { get; set; } // e.g., "/target/path/"
}
