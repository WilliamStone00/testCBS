using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class PostedEntry
    {

        public string Id { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string BranchCode { get; set; }
    }
}
