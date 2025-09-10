using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class CreditAccountingEntry
    {
        public DateTime Date { get; set; }

        [Required]
        public string EntryType { get; set; }
        [Required]
        public string  AccountId { get; set; }
         
        [PositiveAmountValidator]
        public decimal Amount { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string ReferenceNumber { get; set; }

        public string SourceDocumentUrl { get; set; }



        //public string UserId  { get; set; }

        //public List<string> Approvals { get; set; }

        //public string Notes { get; set; }

        public CreditAccountingEntry(DateTime date0, string entryType, string account, decimal amount, string description)
        {
            this.Date = date0;
            EntryType = entryType;
            
            Amount = amount;
            Description = description;
            SourceDocumentUrl= "";
            ReferenceNumber = "";

        }
        public CreditAccountingEntry()
        {
                
        }

        public ManualAccountingEntryDto ConvertToManualAccountingEntryDto() 
        {
            return new ManualAccountingEntryDto
            {
                TransactionReferenceId = ReferenceNumber,
                Amount = Amount,
                EntryType = this.EntryType,
                DebitAccountId =  AccountId ,
                Naration = Description,


            };
        } // TODO: add validation her
    }

 
}
