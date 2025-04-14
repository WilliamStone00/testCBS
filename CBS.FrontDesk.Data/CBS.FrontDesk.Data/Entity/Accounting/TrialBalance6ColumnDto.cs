using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
        public string PrintersName { get; set; }
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
        public double beginningDebitBalance { get; set; }
        public double beginningCreditBalance { get; set; }
        public double debitBalance { get; set; }
        public double creditBalance { get; set; }
        public double endDebitBalance { get; set; }
        public double endCreditBalance { get; set; }
        public double totalBeginningDebitBalance { get; set; }
        public double totalBeginningCreditBalance { get; set; }
        public double totalDebitBalance { get; set; }
        public double totalCreditBalance { get; set; }
        public double totalEndDebitBalance { get; set; }
        public double totalEndCreditBalance { get; set; }
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

        public List< GeneralLedgerDto> ConvertToGeneralLedgerDto(AccountingGeneralLedgerDetails modelAcccount)
        {
            List < GeneralLedgerDto > list = new List<GeneralLedgerDto>();
   
            foreach (var item in modelAcccount.LedgerDetails)
            {
               
                foreach (var entry in item.AccountingEntries)
                {
               
                    var model = new GeneralLedgerDto
                    {

                        EntityType = this.EntityType,
                        FromDate = DateTime.Parse(this.FromDate.ToString("yyyy-MM-dd")),
                        ToDate = this.ToDate.ToString("yyyy-MM-dd"),
                        BranchName = this.BranchName,
                        BranchLocation = this.BranchLocation,
                        BranchAddress = this.BranchAddress,
            
                        Capital = this.Capital,
                        ImmatriculationNumber = this.ImmatriculationNumber,
                        WebSite = this.WebSite,
                        BranchTelephone = this.BranchTelephone,
                        HeadOfficeTelePhone = this.HeadOfficeTelePhone,
                        BranchCode = this.BranchCode,
                        Name = this.Name,
                        Location = this.Location,
                        Address = this.Address
                    };
                    model.TotalDebit = item.AccountingEntries.Sum(x=>x.DrAmount);
                    model.TotalCredit = item.AccountingEntries.Sum(x => x.CrAmount);
                    model.NumberCredit = item.AccountingEntries.Count();
                    model.AccountName = item.AccountName;
                    model.AccountNumber = item.AccountNumber;
                    model.CurrentBalance = item.BeginningBalance;
                    model.Currency = "XAF(BEAC CEMAC)";
                    model.Debit =  entry.DrAmount;
                    model.Credit = entry.CrAmount;
                    model.Balance = entry.CurrentBalance;
                    model.Description = entry.Description;
                    model.Reference = entry.ReferenceID;
                    model.Representative = entry.Representative;
                    model.EntryDate = entry.EntryDate.ToString("yyyy-MM-dd");
                
                    list.Add(model);
                }
           
            }

            return list;
        }
    }
    public class GeneralLedgerDto
    {
        public string EntityType { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Address { get; set; }
        public DateTime FromDate { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal BeginningBalance { get; set; }
        public decimal CurrentBalance { get; set; }
        public string BranchName { get; set; }
        public string BranchLocation { get; set; }
        public string BranchAddress { get; set; }
        public string Capital { get; set; }
        public string ImmatriculationNumber { get; set; }
        public string WebSite { get; set; }
        public string BranchTelephone { get; set; }
        public string HeadOfficeTelePhone { get; set; }
        public string LogoPath { get; set; }
        public string PrintersName { get; set; }
        public string BranchCode { get; set; }
        public string Reference { get; set; }
        public string Representative { get; set; }
        public string ToDate { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
        public string EntryDate { get; set; }
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public double NumberCredit { get; set; }
    }

 


    public class LedgerDetails
    {

        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal BeginningBalance { get; set; }
        public string Currency { get; set; }
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
