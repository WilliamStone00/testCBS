using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{


    public class GetListAccountGLQuery  
    {
        public DateTime ToDate { get; set; }
        public DateTime FromDate { get; set; }
        public string BranchId { get; set; }
        public List<string> AccountIds { get; set; }
    }
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class TrialBalance6ColumnDto
    {
        public string entityId { get; set; }
        public string entityType { get; set; }
        public DateTime fromDate { get; set; }
        public DateTime toDate { get; set; }
        public string branchName { get; set; }
        public object branchLocation { get; set; }
        public string branchAddress { get; set; }
        public string capital { get; set; }
        public string immatriculationNumber { get; set; }
        public string webSite { get; set; }
        public string branchTelephone { get; set; }
        public string headOfficeTelePhone { get; set; }
        public string name { get; set; }
        public string location { get; set; }
        public string address { get; set; }
        public string accountNumber { get; set; }
        public string accountName { get; set; }
        public string beginningDebitBalance { get; set; }
        public string beginningCreditBalance { get; set; }
        public string debitBalance { get; set; }
        public string creditBalance { get; set; }
        public string endDebitBalance { get; set; }
        public string endCreditBalance { get; set; }
        public object totalBeginningDebitBalance { get; set; }
        public object totalBeginningCreditBalance { get; set; }
        public object totalDebitBalance { get; set; }
        public object totalCreditBalance { get; set; }
        public object totalEndDebitBalance { get; set; }
        public object totalEndCreditBalance { get; set; }
        public string cartegory { get; set; }

        public List<TrialBalanceDto> ConvertToExcelTrialBalance(List<TrialBalance6ColumnDto> trialBalances)
        {
            List < TrialBalanceDto > trialBalanceses = new List<TrialBalanceDto> ();
            foreach (var item in trialBalances)
            {
                trialBalanceses.Add(item.ConvertToTrialBalance());
            }
            return trialBalanceses;
        }

        private TrialBalanceDto ConvertToTrialBalance()
        {
            return new TrialBalanceDto 
            {
                accountNumber = this.accountNumber,
                accountName = this.accountNumber,
                beginningDebitBalance = this.beginningDebitBalance,
                beginningCreditBalance = this.beginningCreditBalance,
                debitBalance = this.debitBalance,
                creditBalance = this.creditBalance,
                endDebitBalance = this.endDebitBalance,
                endCreditBalance = this.endCreditBalance,


            };
        }
    }
    public class TrialBalance6ColumnDtoServiceResponse
    {
        public List<TrialBalance6ColumnDto> data { get; set; }
        public List<object> errors { get; set; }
        public int statusCode { get; set; }
        public string statusDescription { get; set; }
        public string message { get; set; }
        public string status { get; set; }
    }

    public class AccountingGeneralLedger
    {
        public string EntityId { get; set; }
        public string EntityType { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string BranchName { get; set; }
        public string BranchLocation { get; set; }
        public string BranchAddress { get; set; }
        public string Capital { get; set; }
        public string ImmatriculationNumber { get; set; }
        public string WebSite { get; set; }
        public string BranchTelephone { get; set; }
        public string HeadOfficeTelePhone { get; set; }
        public string BranchCode { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Address { get; set; }
        public List<AccountingEntryDto> AccountingEntries { get; set; }
        public string MainAccountNumber { get; set; }
    }

    public class AccountingGeneralLedgerDetails
    {
        public string EntityId { get; set; }
        public string EntityType { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string BranchName { get; set; }
        public string BranchLocation { get; set; }
        public string BranchAddress { get; set; }
        public string Capital { get; set; }
        public string ImmatriculationNumber { get; set; }
        public string WebSite { get; set; }
        public string BranchTelephone { get; set; }
        public string HeadOfficeTelePhone { get; set; }
        public string BranchCode { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Address { get; set; }
        public string MainAccountNumber { get; set; }
        public List<LedgerDetails> LedgerDetails { get; set; }
    }
    public class LedgerDetails
    {

        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public List<AccountingEntryDto> AccountingEntries { get; set; }
    }
    public class AccountingGeneralLedgerServiceResponse
    {
        public AccountingGeneralLedgerDetails data { get; set; }
        public List<object> errors { get; set; }
        public int statusCode { get; set; }
        public string statusDescription { get; set; }
        public string message { get; set; }
        public string status { get; set; }
    }


    public class GLServiceResponse
    {
        public List<AccountingEntryDto> data { get; set; }
        public List<object> errors { get; set; }
        public int statusCode { get; set; }
        public string statusDescription { get; set; }
        public string message { get; set; }
        public string status { get; set; }
    }


}
