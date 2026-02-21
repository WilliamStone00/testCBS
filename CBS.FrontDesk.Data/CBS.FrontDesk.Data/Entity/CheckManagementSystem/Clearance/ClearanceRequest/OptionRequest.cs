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
        public string BranchName { get; set; }
        public string CustomerId { get; set; }

        public string CheckBookNumber { get; set; } = null ;


        public int? CheckBookPageNumber { get; set; } 
        public string AccountNumber { get; set; }

        public DateTime CreatedDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
  

        public string RequestNote { get; set; }
        public string ExternalChequeNumber { get; set; }
        public string ExternalBankName { get; set; }
        
        public bool ChequeType { get; set; }
  

        public string ExternalBankCode { get; set; }      // Code banque (SWIFT / BIC)
        public string ExternalBranchCode { get; set; }    // Code agence
        public string ExternalAccountNumber { get; set; }
        public string ExternalBranchName { get; set; }
        public decimal ToAccountNumber { get; set; }

        public bool UseOverdraft { get; set; }
        public string CheckBookId { get; set; }

        public decimal CurrentClearanceAmount { get; set; }
        public decimal RemainingAmount { get; set; }

       
        public string BranchCode { get; set; }

 

        public string ApprovalNote { get; set; }
        public string ReviewNote { get; set; }
        public string RejectionNote { get; set; }
      
        public string PaymentStatus { get; set; }

        public string ClearStatement { get; set; }

        public string ApprovedBy { get; set; }
        public string ClearedBy { get; set; }
        public string RejectedBy { get; set; }

        public DateTime SubmissionDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public DateTime? ClearedDate { get; set; }
        public DateTime? RejectionDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        
        public decimal DeduceAmount { get; set; }
        public decimal ReceiveAmount { get; set; }
        public decimal Charges { get; set; }

       

        public string CheckBook { get; set; }



        public bool IsExternalCheck { get; set; }

        

       

        public int? ExternalCheckLeaf { get; set; }

       

        public string ExternalAccountName { get; set; }

        

        public string ExternalPlaceIssue { get; set; }

        public DateTime? ExternalDateIssue { get; set; }

        public string ExternalLocation { get; set; }

    
        public List<ClearanceRequestImage> Documents { get; set; }

    }


    public class ClearanceResponce
    {
        public string Id { get; set; }


        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string CustomerId { get; set; }

        public string CheckBookNumber { get; set; } = null;
        public DateTime? CreatedDate { get; set; }

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


      
        public string BranchCode { get; set; }

        
        public string ApprovalNote { get; set; }
        public string ReviewNote { get; set; }
        public string RejectionNote { get; set; }
      
        public string PaymentStatus { get; set; }

        public string ClearStatement { get; set; }

        public string ApprovedBy { get; set; }
        public string ClearedBy { get; set; }
        public string RejectedBy { get; set; }

        public DateTime? SubmissionDate { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public DateTime? ClearedDate { get; set; }
        public DateTime? RejectionDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
       
        public decimal DeduceAmount { get; set; }
        public decimal ReceiveAmount { get; set; }
        public decimal Charges { get; set; }
      
        

        public string CheckBook { get; set; }

        public bool IsExternalCheck { get; set; }

        
        public int? ExternalCheckLeaf { get; set; }

       

        public string ExternalAccountName { get; set; }

       

        public string ExternalPlaceIssue { get; set; }

        public DateTime? ExternalDateIssue { get; set; }

        public string ExternalLocation { get; set; }

     

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

   


   

