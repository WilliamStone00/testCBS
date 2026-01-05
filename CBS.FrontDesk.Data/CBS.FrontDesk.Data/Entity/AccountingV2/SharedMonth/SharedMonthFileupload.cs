using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CBS.FrontDesk.Data.Entity.AccountingV2.SharedMonth
{
    public class SharedMonthFileupload
    {
        public string BranchId { get; set; }
        public HttpPostedFileBase file { get; set; }
        public string  FileType { get; set; }
        public string Month { get; set; }
        public string AccountType { get; set; }
        public decimal InterestRate { get; set; }
    }
    public class SharedMonthFileReponse
    {
        public bool HasPassed { get; }
        public string Message { get; }
    }

}
