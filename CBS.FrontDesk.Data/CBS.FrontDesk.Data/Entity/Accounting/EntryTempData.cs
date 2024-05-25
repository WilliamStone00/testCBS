using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class EntryTempData
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        [Required]
        public string BookingDirection { get; set; }
        [PositiveAmountValidator]
        public decimal Amount { get; set; }
        public string AccountBalance { get; set; }

        public string Description { get; set; }
        [Required]
        public string Reference { get; set; }
    }
    public class EntryDescription
    {
        [Required]
        public string Description { get; set; }
        [Required]
        public string Reference { get; set; }
    }
    public class EntryApproval
    {

        public string Id { get; set; }

        public bool HasApproved { get; set; }
    }
    public class EntryTempDataResult
    {

        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public decimal SumDebit { get; set; }
        public decimal SumCredit { get; set; }
        public string BookingDirection { get; set; }
        public decimal Difference { get; set; }
        public decimal Amount { get; set; }
        public string Reference { get; set; }
        public string Id { get; set; }
    }
    public class ManuallyJournalEntryDataSet
    {
        public EntryTempData EntryTempData { get; set; } = new EntryTempData();
        public Data.Account Account { get; set; }= new Account();
        public EntryDescription EntryDescription { get; set; } = new EntryDescription();
        public List<EntryTempData> EntryTempDatas { get; set; }= new List<EntryTempData>(){ };

        public List<PostedEntry> PostedEntries { get; set; }= new List<PostedEntry>();
        public List<EntryTempDataResult> EntryTempDataResult { get; set; } = new List<EntryTempDataResult> { 
    
        };   
        public List<Account> Accounts { get; set; }= new List<Account>();
        public string ServiceOption { get; set; }
        public string Action { get; set; }
        public string Key { get; set; }
        public bool HasApproved { get; set; }
    }
}
