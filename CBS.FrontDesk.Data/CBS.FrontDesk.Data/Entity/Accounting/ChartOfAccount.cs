using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class ChartOfAccount
    {
        public string Id { get; set; }
        [Required]
        public string AccountNumber { get; set; }
        [Required]
        public string LabelEn { get; set; }
      
        //public string OperationDirection { get; set; }
        [Required]
        public string AccountCartegoryId { get; set; }
        [Required]
        public string LabelFr { get; set; }
        public bool IsBalanceSheetAccount { get; set; }
        public bool CanBeNegative { get; set; }
        public bool IsDebit { get; set; }
        public bool CreateAccount { get; set; }
        public string ParentAccountNumber { get; set; }
        public string ParentAccountId { get; set; }
        public bool IsForUpdate { get; set; }
        public string AccountNumberCamCCUL { get; set; }
        public string AccountNumberAffiliate { get; set; }
        public string GeneralRepresentation { get; set; }

    }
   
}
