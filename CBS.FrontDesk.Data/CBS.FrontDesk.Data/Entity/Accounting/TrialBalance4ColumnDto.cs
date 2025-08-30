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
        public string PrintersName { get; set; }
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
        public string BeginningBookingDirection { get; set; }
        public string EndingBookingDirection { get; set; }
        public string  Address { get; set; }
        public string  AccountNumber { get; set; }
        public string  AccountName { get; set; }
        public double BeginningBalance { get; set; }
        public double DebitBalance { get; set; }
        public double CreditBalance { get; set; }
        public double EndingBalance { get; set; }
        public double totalBeginningBalance { get; set; }
        public double totalDebitBalance { get; set; }
        public double totalCreditBalance { get; set; }
        public double totalEndingBalance { get; set; }
    }
    public class AccountLedgerDtosss
    {
        public double totalCreditBalance { get; set; }
        public double totalEndingBalance { get; set; }
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
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string Description { get; set; }
        public string Reference { get; set; }
        public string EntryDate { get; set; }
        public string ValueDate { get; set; }
        public decimal SumDebit { get; set; }
        public decimal SumCredit { get; set; }
        public decimal NumberEntries { get; set; }
        public decimal NumberDebit { get; set; }
        public decimal NumberCredit { get; set; }
        public string PrintersName { get;   set; }
        public string Auxilary { get;   set; }
        public string BranchCode { get;   set; }
    }
    public class BalanceSheetAccount : BalanceSheet
    {
        public double Gross { get; set; } //Gross

        public double Amort_Dep { get; set; } //Amort_Dep
    }

    public class BalanceSheet
    {
        public string Id { get; set; }
        public string Reference { get; set; }
        // Account Holder
        public string Description { get; set; } //Naration


        public double Net { get; set; } //NET

        public double Net_1 { get; set; } //NET

        public string Category { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
   

    public class BalanceSheetData :ReportHeader
    {
    
        public List<BalanceSheetAccount> Accounts { get; set; }
        public List<TrialBalanceDt> TrialBalanceDt { get; set; }
        public string Message { get; set; }


        public List<BalanceSheetInfo> ConvertToBalanceSheetInfo(string PrintersName, string Date, string categoryA, string categoryB)
        {
            var accountsA = this.Accounts
                .Where(a => a.Category.ToUpper().Contains(categoryA.ToUpper()))
                .Select(a => new
                {
                    JoinKey = a.Reference.Substring(Math.Max(0, a.Reference.Length - 2)),
                    ANet = a.Net,
                    ANet_1 = a.Net_1,
                    Gross = a.Gross,
                    Amort_Prev = a.Amort_Dep,
                    ACode = a.Reference,
                    ANaration = a.Description,
                }).OrderBy(x => x.ACode);

            var accountsB = this.Accounts
                .Where(b => b.Category.ToUpper().Contains(categoryB.ToUpper()))
                .Select(b => new
                {
                    JoinKey = b.Reference.Substring(Math.Max(0, b.Reference.Length - 2)),
                    LNet = b.Net,
                    LNet_1 = b.Net_1,
                    LCode = b.Reference,
                    LNaration = b.Description
                }).OrderBy(x => x.LCode);

            return accountsA.GroupJoin(accountsB,
                a => a.JoinKey,
                b => b.JoinKey,
                (a, bGroup) => new { A = a, BGroup = bGroup })
                .SelectMany(
                x => x.BGroup.DefaultIfEmpty(),
                (a, b) => new BalanceSheetInfo
                {
                    ACode = a.A.ACode,
                    Amort_Prev = (decimal)a.A.Amort_Prev,
                    Gross = (decimal)a.A.Gross,
                    ANaration = a.A.ANaration,
                    LNet = (decimal)(b?.LNet ?? 0),
                    LNet_1 = (decimal)(b?.LNet_1 ?? 0),
                    ANet = (decimal)(a.A.ANet),
                    ANet_1 = (decimal)(a.A.ANet_1),
                    LCode = b?.LCode,
                    LNaration = b?.LNaration,
                    Address = this.Address,
                    BranchCode = this.BranchCode,
                    BranchName = this.BranchName,
                    BranchTelephone = this.BranchTelephone,
                    Name = this.Name,

                    PrintersName = PrintersName,
                    ToDate = this.ToDate
                })
            .ToList();
        }
    }
    public class BalansheetRpt
    {
        // Entity Information
        public string EntityId { get; set; }
        public string EntityType { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Location { get; set; }
        public string ImmatriculationNumber { get; set; }
        public string WebSite { get; set; }
        public string HeadOfficeTelePhone { get; set; }
        public decimal Capital { get; set; }
        public string LogoPath { get; set; }
        public string PrintersName { get; set; }
        // Report Period
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime EntryDate { get; set; }
        // Branch Information
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string BranchLocation { get; set; }
        public string BranchAddress { get; set; }
        public string BranchTelephone { get; set; }



        // Account Information
        public string MainAccountNumber { get; set; }
        public string Category { get; set; }

        // Assets
        public string ACode { get; set; }
        public string ANaration { get; set; }
        public decimal Gross { get; set; }
        public decimal Amort_Prev { get; set; }
        public decimal ANet { get; set; }
        public decimal ANet_1 { get; set; }
        public decimal TotalAsset { get; set; }

        // Liabilities
        public string LCode { get; set; }
        public string LNaration { get; set; }
        public decimal LNet { get; set; }
        public decimal LNet_1 { get; set; }
        public decimal TotalLiabilityEquity { get; set; }
    }

    public class PandLInfo : ReportHeader
    {
        // Entity Information

        public string LogoPath { get; set; }
        public string PrintersName { get; set; }
        // Report Period
        public DateTime EntryDate { get; set; }
        // Branch Information


        public string Category { get; set; }
        public string Code { get; set; }
        public string Naration { get; set; }
        public decimal AmountN { get; set; }
        public decimal AmountN_1 { get; set; }

    }
    public class IncomeAndExpenseAccount
    {

        public string Code { get; set; }
        // Account Holder
        public string Naration { get; set; } //


        public string Category { get; set; }
        public decimal AmountN { get; set; }

        public decimal AmountN_1 { get; set; }
    }

    public class IncomeAndExpenseDto : ReportHeader
    {


        public DateTime Date { get; set; }

        public List<IncomeAndExpenseAccount> Accounts { get; set; } = new List<IncomeAndExpenseAccount>();

        public List<PandLInfo> ConvertToIncomeStatementModel(string printersName)
        {
            // Common properties for projection
            var commonProperties = new
            {
                ToDate = this.ToDate,
                Address = this.Address,
                BranchCode = this.BranchCode,
                BranchName = this.BranchName,
                BranchTelephone = this.BranchTelephone,
                Name = this.Name,
                PrintersName = printersName
            };

            // Single query for both income and expense accounts
            var pandLInfos = this.Accounts
                .Where(a => a.Category != null &&
                           (a.Category.ToUpper().Contains("INCOME") ||
                            a.Category.ToUpper().Contains("EXPENSE")))
                .Select(p => new PandLInfo
                {
                    Code = p.Code,
                    Naration = p.Naration,
                    AmountN = p.AmountN,
                    AmountN_1 = p.AmountN_1,
                    Category = p.Category,
                    PrintersName = commonProperties.PrintersName,
                    ToDate = commonProperties.ToDate,
                    Address = commonProperties.Address,
                    BranchCode = commonProperties.BranchCode,
                    BranchName = commonProperties.BranchName,
                    BranchTelephone = commonProperties.BranchTelephone,
                    Name = commonProperties.Name
                })
                .OrderBy(x => x.Code)
                .ToList();

            return pandLInfos;
        }
    }

    public class BalanceSheetInfo
    {
        // Entity Information
        public string EntityId { get; set; }
        public string EntityType { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Location { get; set; }
        public string ImmatriculationNumber { get; set; }
        public string WebSite { get; set; }
        public string HeadOfficeTelePhone { get; set; }
        public decimal Capital { get; set; }
        public string LogoPath { get; set; }
        public string PrintersName { get; set; }
        // Report Period
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime EntryDate { get; set; }
        // Branch Information
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string BranchLocation { get; set; }
        public string BranchAddress { get; set; }
        public string BranchTelephone { get; set; }

        // Assets
        public string ACode { get; set; }
        public string ANaration { get; set; }
        public decimal Gross { get; set; }
        public decimal Amort_Prev { get; set; }
        public decimal ANet { get; set; }
        public decimal ANet_1 { get; set; }
        public decimal TotalAsset { get; set; }

        // Liabilities
        public string LCode { get; set; }
        public string LNaration { get; set; }
        public decimal LNet { get; set; }
        public decimal LNet_1 { get; set; }
        public decimal TotalLiabilityEquity { get; set; }
    }
}
