using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AccountingV2.CashReconciliation
{
    //public class CashAndVaultInit
    //{
    
    //public string OwnerName { get; set; }
    //public string TillName { get; set; }
    //public string Narration { get; set; }
    //public string BranchId { get; set; }
    //public DateTime? AccountingDate { get; set; }
    //public string ReconciliationType { get; set; }
    //public decimal DifferenceInAmount { get; set; }
    //public decimal GlBalance { get; set; }
    //    public decimal OGlBalance { get; set; }

    //    public string SourceGLId { get; set; }
    //public string DestinationGLId { get; set; }
    // public decimal Amount { get; set; }

    //}

    

   public class CashAndVaultInit
    {
        public string OwnerName { get; set; }
        public string TillName { get; set; }
        public string Narration { get; set; }
        public string BranchId { get; set; }
        public DateTime? AccountingDate { get; set; }
        public string ReconciliationType { get; set; }
        public decimal DifferenceInAmount { get; set; }
        public decimal CashInHand { get; set; }
        public decimal GlBalance { get; set; }
        public string SourceGLId { get; set; }
       
        public string DestinationGLId { get; set; }
       
        public decimal OGlBalance { get; set; }
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


    public class TrialBalanceEntry
    {
        public string BranchAccountId { get; set; }
        public string TrialBalanceStagingId { get; set; }
        public decimal TrialBalanceBalance { get; set; }
        public string TrialSide { get; set; }
    }

}
