using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.Base
{
    public class BranchInfo 
    {
        public string Id { get; set; }
        public string BranchName { get; set; }
        public string BranchCode     { get; set; }

    }

    public class BranchAccountInfo
    {
        public string Id { get; set; }
        public string BranchAccountName { get; set; }
        public string code { get; set; }
        public string NameEn { get; set; }
        public string NameFr { get; set; }
        public string Class { get; set; }

    }


}
