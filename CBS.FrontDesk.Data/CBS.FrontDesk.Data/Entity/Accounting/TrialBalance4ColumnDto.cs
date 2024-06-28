using System;
using System.Collections.Generic;
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
    }
}
