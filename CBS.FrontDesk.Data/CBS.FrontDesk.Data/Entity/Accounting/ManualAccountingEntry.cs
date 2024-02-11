using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class ManualAccountingEntry
    {
        public DateTime Date { get; set; }

        [Required]
        public string EntryType { get; set; }
        [Required]
        public string SourceAccountId { get; set; }
        [Required]
        public string destinationAccountId { get; set; }
        [PositiveAmountValidator]
        public decimal Amount { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string ReferenceNumber { get; set; }

        //public string SourceDocumentUrl { get; set; }



        //public string UserId  { get; set; }

        //public List<string> Approvals { get; set; }

        //public string Notes { get; set; }

        public ManualAccountingEntry(DateTime date0, string entryType, string account, decimal amount, string description)
        {
            this.Date = date0;
            EntryType = entryType;
            
            Amount = amount;
            Description = description;
            //SourceDocumentUrl= "";
            ReferenceNumber = "";

        }
        public ManualAccountingEntry()
        {
                
        }

        public ManualAccountingEntryDto ConvertToManualAccountingEntryDto() 
        {
            return new ManualAccountingEntryDto
            {
                TransactionReferenceId = ReferenceNumber,
                Amount = Amount,
                EntryType = this.EntryType,
                DebitAccountId =(EntryTypeOperation.Debit == this.EntryType.ToUpper())?SourceAccountId:destinationAccountId,
                CreditAccountId = (EntryTypeOperation.Credit == this.EntryType.ToUpper()) ? SourceAccountId : destinationAccountId,
                Naration = Description,


            };
        } // TODO: add validation her
    }


    public class ManualAccountingEntryDto
    {
       
        public string TransactionReferenceId { get; set; }
        public decimal Amount { get; set; }
        public string EntryType { get; set; }
        public string DebitAccountId { get; set; }
        public string CreditAccountId { get; set; }
        public string Naration { get; set; }
     
    }
}
