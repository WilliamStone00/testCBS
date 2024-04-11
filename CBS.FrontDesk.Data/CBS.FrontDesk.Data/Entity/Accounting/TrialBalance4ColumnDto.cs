using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
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
        public decimal totalBeginningBalance { get; set; }
        public decimal totalDebitBalance { get; set; }
        public decimal totalCreditBalance { get; set; }
        public decimal totalEndingBalance { get; set; }
    }
}
