using CBS.FrontDesk.Data.Entity.Accounting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
 
    public class CashInfusion
    {

        [Required]
        public string SourceAccountId { get; set; }
       
        [PositiveAmountValidator]
        public decimal Amount { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string ReferenceNumber { get; set; }

        //public string SourceDocumentUrl { get; set; }
          public string EntryType  { get; set; }

        //public List<string> Approvals { get; set; }

        public string CurrencyCode { get; set; }
        public CashInfusion()
        {
                
        }
        public CashInfusion( string entryType, string account, decimal amount, string description)
        {
    
     

            Amount = amount;
            Description = description;
            //SourceDocumentUrl= "";
            ReferenceNumber = "";

        }
 
    }
}
