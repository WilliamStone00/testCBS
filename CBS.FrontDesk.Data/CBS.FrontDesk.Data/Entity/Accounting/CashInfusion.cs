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
       
      //  [PositiveAmountValidator]
        public decimal Amount { get; set; }
       // [Required]
        public string RequestMessage { get; set; }
        //   [Required]
        public string ReferenceNumber { get; set; } 
        public string CurrentOperation { get; set; }
        public CashInfusion()
        {
                
        }
        public CashInfusion( string entryType, string account, decimal amount, string description)
        {
    
     

            Amount = amount;
            RequestMessage = description;
            //SourceDocumentUrl= "";
            ReferenceNumber = "";

        }

        public CashReplenimentRequest ConvertToCashReplenimentRequest()
        {
          return new CashReplenimentRequest { Amount = Amount, RequestMessage = RequestMessage, ReferenceId = ReferenceNumber, IssuedBy="Not-Set" };
        }


    }
}
