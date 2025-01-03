using CBS.FrontDesk.Data.Entity.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class AccountingStatementDto
    {
 
        public List<Account> Accounts { get; set; } = new List<Account>();
        public List<Branch> Branches { get; set; } = new List<Branch>();
        public List<AccountDto> AccountDtos { get; set; } = new List<AccountDto>();
        public List<AccountingEntryDto> AccountingEntryDtos { get; set; } = new List<AccountingEntryDto>();
        public AccountingGeneralLedgerDetails AccountingGeneralLedger { get; set; } = new AccountingGeneralLedgerDetails();
        public Branch Branch { get; set; } = new  Branch();
        public Account Account  { get; set; } = new Account();
   
    }
    public class ReportInfo
    {
        public string Id { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
        public string DownloadPath { get; set; }
        public string FileType { get; set; }
        public string FullPath { get; set; }
        public string Size { get; set; }
        public string UserId { get; set; }
        public string ReportType { get; set; }
        public string UserName { get; set; }
        public string BranchName { get; set; }
        public string BranchId { get; set; }
        public DateTime CreatedDate { get; set; }

    }
    public class FileReportInfoDto
    {
        public byte[] FileData { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public string ErrorMessage { get; set; }
    }
}
