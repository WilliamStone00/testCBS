using CBS.FrontDesk.Data.Entity.Config;
using System;
using System.Xml.Linq;

namespace CBS.FrontDesk.Data
{
    public class TrailBalanceUploud 
    {
        public string Id { get; set; }
        public string BranchName { get; set; }
        public string Account_Left { get; set; }
        public string UserName { get; set; }

        public bool UploadStatus { get; set; }
        public string FilePath { get; set; }


    }

}