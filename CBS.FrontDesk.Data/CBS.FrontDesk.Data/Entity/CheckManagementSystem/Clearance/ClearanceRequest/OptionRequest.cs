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
        public bool External { get; set; }
        [Required(ErrorMessage = "Branch is required")]
        public string BranchId { get; set; } 

        [Required(ErrorMessage = "CheckBook ID is required")]
        public string CheckBookNumber { get; set; } = null ;

        [Required(ErrorMessage = "Page number is required")]
        public int CheckBookPageNumber { get; set; } 
        public string AccountNumber { get; set; }

        public bool DepositToAccount { get; set; }
        public bool SameDayProcessing { get; set; }
        public string NationalIdNumber { get; set; }
        public string ChequeBranchId { get; set; }
        public string ReceivingBranchId { get; set; }
        public string MemberReference { get; set; }
        public DateTime? NationalIdCreationDate { get; set; } = DateTime.Now;
        public DateTime? NationalIdExpirationDate { get; set; } = DateTime.Now;
        public DateTime? ChequeCreationDate { get; set; } = DateTime.Now;
        public DateTime? ChequeExpirationDate { get; set; } = DateTime.Now;
        public decimal ChequeAmount { get; set; }
        public string Status { get; set; }
        //public string DepositToAccount { get; set; }
        public string ChequeImagePath { get; set; }
        public Discount Discount { get; set; }

        public string RequestNote { get; set; }
        public string ExternalChequeNumber { get; set; }
        public string ExternalBankName { get; set; }
        public string Amount { get; set; }
        public string ChequeType { get; set; }
        public string CustomerId { get; set; }

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
