using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
    public class FundingLine
    {
        public string id { get; set; }
        public string name { get; set; }
        public DateTime beginDate { get; set; }
        public DateTime endDate { get; set; }
        public double amount { get; set; }
        public string Description { get; set; }
        public string currencyId { get; set; }
        public string AccountingRuleId { get; set; }
        public string OrganizationId { get; set; }
        public string BankId { get; set; }
        public string BranchId { get; set; }

    }
}
