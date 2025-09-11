using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
    public class WriteOffLoan
    {
        public string Id { get; set; }
        public string LoanId { get; set; }
        public double OutstandingLoanBalance { get; set; }
        public double AccruedInterests { get; set; }
        public double AccruedPenalties { get; set; }
        public int PastDueDays { get; set; }
        public double OverduePrincipal { get; set; }
        public string Comment { get; set; }
        public string WriteOffMethod { get; set; }//Enum
        public string AccountingRuleId { get; set; }
        public string OrganizationId { get; set; }
        public string BankId { get; set; }
        public string BranchId { get; set; }
    }
}
