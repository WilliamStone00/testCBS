using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class CashApprovalResponse
    {
        public string Id { get; set; }
        public string ApprovedMessage { get; set; }
        public string Status { get; set; }
        public bool IsApproved { get; set; }
        public decimal ApprovedAmount { get; set; }
        public string CorrespondingBranchId { get; set; }
        public string BranchId { get;   set; }
        
         public string CashRequisitionType { get; set; }
        public string BranchCode { get;   set; }
        public string AccountId { get;   set; }
    }
     
 
}

