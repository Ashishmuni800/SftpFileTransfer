using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Crypto.Agreement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpFileTransfer
{
    public class SetTime
    {
        private readonly IConfiguration _configuration;
        public SetTime(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public int GetTimeforTransferReqFileScheduler()
        {
            string SetTimeforTrans = _configuration["IsSchedulerSartTimeForPayRequest"];
            if (!string.IsNullOrEmpty(SetTimeforTrans))
            {
                return int.Parse(SetTimeforTrans);
            }
            return 0;
        }
        public int GetTimeforTransferScheduler()
        {
            string SetTimeforTrans = _configuration["IsSchedulerSartTimeForPayResponse"];
            if (!string.IsNullOrEmpty(SetTimeforTrans))
            {
                return int.Parse(SetTimeforTrans);
            }
            return 0;
        }
        public int GetTimeforTransferNEFTReturnFileScheduler()
        {
            string SetTimeforTrans = _configuration["IsSchedulerSartTimeForPayNEFTReturn"];
            if (!string.IsNullOrEmpty(SetTimeforTrans))
            {
                return int.Parse(SetTimeforTrans);
            }
            return 0;
        }
    }
}
