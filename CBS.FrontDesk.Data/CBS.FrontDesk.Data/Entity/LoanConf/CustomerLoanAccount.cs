using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
    public class CustomerLoanAccount
    {
        public string id { get; set; }
        public string customerId { get; set; }
        public string balance { get; set; }
        public string previouseBalance { get; set; }
        public string lastloanId { get; set; }
        public string encryptionCode { get; set; }
        public string organizationId { get; set; }
        public string branchId { get; set; }
        public string bankId { get; set; }
    }

}
