using System.Collections.Generic;

public class TransferJob
{
    public string IsSchedulerSartTimeForPayRequest { get; set; }
    public string IsSchedulerSartTimeForPayResponse { get; set; }
    public string IsSchedulerSartTimeForPayNEFTReturn { get; set; }
    public bool IsSchedulerActiveForPayRequest { get; set; }
    public bool IsSchedulerActiveForPayResponse { get; set; }
    public bool IsSchedulerActiveForPayNEFTReturn { get; set; }
    public string SourceHost { get; set; }
    public int SourcePort { get; set; } = 22;
    public string SourceUser { get; set; }
    public string SourcePassword { get; set; }

    public string RemoteDirectory { get; set; } // e.g., "/remote/path/"
    public string RemoteReqFileDirectory { get; set; } // e.g., "/remote/path/"
    public string RemoteNEFTReturnFileDirectory { get; set; } // e.g., "/remote/path/"
    public List<string> FilenamePrefixes { get; set; } // ["N5132Bk", "T5132Bk"]
    public List<string> FilenameNEFTReturnPrefixes { get; set; } // ["N5132Bk", "T5132Bk"]

    public string TargetHost { get; set; }
    public int TargetPort { get; set; } = 22;
    public string TargetUser { get; set; }
    public string TargetPassword { get; set; }
    public string TargetDirectory { get; set; } // e.g., "/target/path/"
    public string TargetReqFileDirectory { get; set; } // e.g., "/target/path/"
    public string TargetNEFTReturnFileDirectory { get; set; } // e.g., "/target/path/"
}
