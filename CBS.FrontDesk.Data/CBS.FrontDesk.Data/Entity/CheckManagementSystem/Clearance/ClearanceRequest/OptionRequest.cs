using CBS.FrontDesk.Data.Entity.CheckManagementSystem.LossManagementSystem;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.Clearance.ClearanceRequest
{
    public class OptionRequest
    {
        public string Id { get; set; }
        
       
        public string BranchId { get; set; }
        public string CustomerId { get; set; }

        public string CheckBookNumber { get; set; } = null ;


        public int? CheckBookPageNumber { get; set; } 
        public string AccountNumber { get; set; }

       
        public decimal Amount { get; set; }
        public string Status { get; set; }
  

        public string RequestNote { get; set; }
        public string ExternalChequeNumber { get; set; }
        public string ExternalBankName { get; set; }
        
        public int ChequeType { get; set; }
  

        public string ExternalBankCode { get; set; }      // Code banque (SWIFT / BIC)
        public string ExternalBranchCode { get; set; }    // Code agence
        public string ExternalAccountNumber { get; set; }
        public string ExternalBranchName { get; set; }
        public decimal ToAccountNumber { get; set; }

        public bool UseOverdraft { get; set; }
        public string CheckBookId { get; set; }

        public decimal CurrentClearanceAmount { get; set; }
        public decimal RemainingAmount { get; set; }

    }


    public class ClearanceResponce
    {
        public string Id { get; set; }


        public string BranchId { get; set; }
        public string CustomerId { get; set; }

        public string CheckBookNumber { get; set; } = null;


        public int? CheckBookPageNumber { get; set; }
        public string AccountNumber { get; set; }


        public decimal Amount { get; set; }
        public string Status { get; set; }


        public string RequestNote { get; set; }
        public string ExternalChequeNumber { get; set; }
        public string ExternalBankName { get; set; }

        public string ChequeType { get; set; }


        public string ExternalBankCode { get; set; }      // Code banque (SWIFT / BIC)
        public string ExternalBranchCode { get; set; }    // Code agence
        public string ExternalAccountNumber { get; set; }
        public string ExternalBranchName { get; set; }
        public decimal ToAccountNumber { get; set; }

        public bool UseOverdraft { get; set; }
        public string CheckBookId { get; set; }

        public decimal CurrentClearanceAmount { get; set; }
        public decimal RemainingAmount { get; set; }

    }

    public class Discount
    {

        public string BranchId { get; set; }
        public string FeeType { get; set; }
        public bool IsCentralized { get; set; }
        public double IssueAmount { get; set; }

        public double NewAmount { get; set; }
        public double Fees { get; set; }


    }


    public class ClearanceRequestImage
    {
        public List<HttpPostedFileBase> AttachedFiles { get; set; }
        public string Id { get; set; }
        public string UrlPath { get; set; }
        public string DocumentName { get; set; }
        public string Extension { get; set; }
        public string BaseUrl { get; set; }
        public string DocumentType { get; set; } 
    }


}
