using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
       public class UploadAccountResult
    {
        public int Account_Present { get; set; }
        public int Total_Account { get; set; }
        public string file_path { get; set; }

        public bool UploadStatus { get; set; }

        public string BranchName { get; set; }

 
    }
    public class UploadAccountResultServiceResponse
    {
        public bool isSuccess { get; set; }
        public string message { get; set; }
        public UploadAccountResult apiResponseData { get; set; }
    }

    public class AccountingEntriesReportResponse
    {
        public AccountingEntriesReport Data { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public List<string> Errors { get; set; }
    }
    public class TrialBalance4ColumnDtoServiceResponse
    {
        public List<TrialBalance4ColumnDto> Data { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public List<string> Errors { get; set; }
    }
    public class TrialBalance4ColumnDto
    {
        public string EntityId { get; set; }
        public string EntityType { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string  BranchName { get; set; }
        public string  BranchLocation { get; set; }
        public string  BranchAddress { get; set; }
        public string Capital { get; set; }
        public string ImmatriculationNumber { get; set; }
        public string WebSite { get; set; }
        public string BranchTelephone { get; set; }
        public string HeadOfficeTelePhone { get; set; }
        public string  Name { get; set; }
        public string  Location { get; set; }
        public string  Address { get; set; }
        public string  AccountNumber { get; set; }
        public string  AccountName { get; set; }
        public string  BeginningBalance { get; set; }
        public string  DebitBalance { get; set; }
        public string  CreditBalance { get; set; }
        public string  EndingBalance { get; set; }
        public object totalBeginningBalance { get; set; }
        public object totalDebitBalance { get; set; }
        public object totalCreditBalance { get; set; }
        public object totalEndingBalance { get; set; }
    }


    public class AccountLedgerDto
    {
    
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string BranchName { get; set; }
        public string BranchLocation { get; set; }

        public string Capital { get; set; }
        public string ImmatriculationNumber { get; set; }
        public string WebSite { get; set; }
        public string BranchTelephone { get; set; }
        public string HeadOfficeTelePhone { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Address { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public Decimal CurrentBalance { get; set; }
        public string BranchId { get; set; }
    }

    public class JournalEntryDto
    {

        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string BranchName { get; set; }
        public string BranchLocation { get; set; }

        public string Capital { get; set; }
        public string ImmatriculationNumber { get; set; }
        public string WebSite { get; set; }
        public string BranchTelephone { get; set; }
        public string HeadOfficeTelePhone { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Address { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string Debit { get; set; }
        public string Credit{ get; set; }
        public string Description { get; set; }
        public string Reference { get; set; }
        public string EntryDate { get; set; }
        public string SumDebit { get; set; }
        public string SumCredit { get; set; }
        public string NumberEntries { get; set; }
        public string NumberDebit { get; set; }
        public string NumberCredit { get; set; }
        public string PrintersName { get;   set; }
        public string Auxilary { get;   set; }
        public string BranchCode { get;   set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class BSAccount
    {
        public string reference { get; set; }
        public string description { get; set; }
        public double amount { get; set; }
        public string cartegory { get; set; }
    }

    public class BalanceSheetData
    {
        public string name { get; set; }
        public string location { get; set; }
        public string address { get; set; }
        public DateTime date { get; set; }
        public double totalAsset { get; set; }
        public double totalLiabilityEquity { get; set; }
        public List<BSAccount> accounts { get; set; }
        public string branchCode { get; set; }
        public string branchName { get; set; }
        public string branchAddress { get; set; }
        public string immatriculationNumber { get; set; }
        public string capital { get; set; }
        public string branchTelephone { get; set; }
        public string headOfficeTelePhone { get; set; }
        public string webSite { get; set; }
     

        public List<BalanceSheetInfo> ConvertToBalanceSheetInfo(string PrintersName, string Date, string categoryA, string categoryB)
        {
            var accountsA = this.accounts
                .Where(a => a.cartegory.ToUpper().Contains(categoryA.ToUpper()))
                .Select(a => new
                {
                    JoinKey = a.reference.Substring(Math.Max(0, a.reference.Length - 2)),
                    Amount = a.amount,
                    Reference = a.reference,
                    Description = a.description
                }).OrderBy(x=>x.Reference);

            var accountsB = this.accounts
                .Where(b => b.cartegory.ToUpper().Contains(categoryB.ToUpper()))
                .Select(b => new
                {
                    JoinKey = b.reference.Substring(Math.Max(0, b.reference.Length - 2)),
                    Amount = b.amount,
                    Reference = b.reference,
                    Description = b.description
                }).OrderBy(x => x.Reference); 

            return accountsA.GroupJoin(accountsB,
                a => a.JoinKey,
                b => b.JoinKey,
                (a, bGroup) => new { A = a, BGroup = bGroup })
            .SelectMany(
                x => x.BGroup.DefaultIfEmpty(),
                (a, b) => new BalanceSheetInfo
                {
                    AmountA = a.A.Amount,
                    ReferenceA = a.A.Reference,
                    DescriptionA = a.A.Description,
                    AmountB = b?.Amount ?? 0,
                    ReferenceB = b?.Reference,
                    DescriptionB = b?.Description,
                    Address = this.address,
                    BranchCode = this.branchCode,
                    BranchName = this.branchName,
                    BranchTelephone = this.branchTelephone,
                    Name= this.name,
                    TotalAsset= this.totalAsset,
                    TotalLiabilities=this.totalLiabilityEquity,
                    PrintersName = PrintersName,
                    ToDate = Date
                })
            .ToList();
        }
    }

  
    public class BalanceSheetInfo
    {
        public string EntityType { get; set; }
        public string Name { get; set; }
        public DateTime FromDate { get; set; }
        public string Address { get; set; }
        public string ReferenceA { get; set; }
        public string BranchName { get; set; }
        public string ToDate { get; set; }
        public string BranchAddress { get; set; }
        public decimal Capital { get; set; }
        public string ImmatriculationNumber { get; set; }
        public string WebSite { get; set; }
        public string BranchTelephone { get; set; }
        public string HeadOfficeTelePhone { get; set; }
        public string LogoPath { get; set; }
        public string DescriptionA { get; set; }
        public double TotalLiabilities { get; set; }
        public double TotalAsset { get; set; }
        public string BranchCode { get; set; }
        public string PrintersName { get; set; }
        public double AmountA { get; set; }
    
        public string DescriptionB { get; set; }
        public string ReferenceB { get; set; }
        public double AmountB { get; set; }
    }
}
