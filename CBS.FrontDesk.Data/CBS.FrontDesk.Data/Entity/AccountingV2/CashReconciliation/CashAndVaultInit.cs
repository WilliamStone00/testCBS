using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AccountingV2.CashReconciliation
{
    public class CashAndVaultInit
    {
        public string BranchId { get; set; }
        public string NameOfCashier { get; set; }
        public string TillName { get; set; }
        public string Naration { get; set; }
        public string SourceGl { get; set; }
        public string DesctinationGl { get; set; }
        public decimal Amount { get; set; }
        public DateTime? AccountingDate { get; set; }
       
    }


    public class AccountDetailDto
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string AccountId { get; set; }
        
    }

    public class AccountGroupDto
    {
        public List<AccountDetailDto> TreasuryAccounts { get; set; }
        public List<AccountDetailDto> DeficitAccounts { get; set; }
    }



}
