using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public enum CashRequisitionType
    {
        ORDER,
        REQUEST

    }
    public class CashReplenimentRequest
    {
        public string Id { get; set; }
        public string ReferenceId { get; set; }
        public decimal AmountRequested { get; set; }
        public string RequestMessage { get; set; }
        [Required]
        public decimal AmountApproved { get; set; }
        [Required]
        public string CurrentOpenOfDayHistoryId { get; set; }
        public string CorrespondingBranchId { get; set; }
        public string IssuedBy { get; set; }
        public string IssuedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public bool IsApproved { get; set; }
        public string BranchId { get; set; }
        public string CurrencyCode { get; set; }
        [Required]
        public string ApprovedMessage { get; set; }
        public string CashRequisitionType { get; set; } = "REQUEST";
        public string Status { get; set; }
        public string ParentCashReplenishId { get; set; } = "XXXXXXXX";
        public string ApprovalCode { get; set; }
        public string CashReplishmentRequestStatus { get; set; } = "Pendding";
        public CashApprovalResponse ConvertToCashApprovalResponse(bool Approved)
        { return new CashApprovalResponse { ApprovedMessage =this.ApprovedMessage, IsApproved=Approved,Id= this.Id }; }

        public CashInfusion ConvertToCashInfusionModel()
        {
            return new CashInfusion { Amount = this.AmountRequested, RequestMessage = this.RequestMessage, CurrentOpenOfDayHistoryId = this.CurrentOpenOfDayHistoryId, Id= this.Id };
        }


        public CashReplenimentRequestDto ConvertToCashReplenimentRequestDto()
        {
            return new CashReplenimentRequestDto
            {
                Id = this.Id,
                AmountRequested = this.AmountRequested,
                RequestMessage = this.RequestMessage,
                ReferenceId = this.ReferenceId,
                IssuedBy = this.IssuedBy,
                IssuedDate = this.IssuedDate,
                ApprovedBy = this.ApprovedBy,
                ApprovedDate = this.ApprovedDate,
                IsApproved = this.IsApproved,
                CurrencyCode = this.CurrencyCode,
                ApprovedMessage = this.ApprovedMessage,
                Status = this.Status,
                IsRejected = false,
                BranchId = this.BranchId,
                CashReplishmentRequestStatus = this.CashReplishmentRequestStatus, 
                CorrespondingBranchId = this.CorrespondingBranchId
            };
        }
    }
    //
    public class CashReplenimentRequestDto: CashReplenimentRequest
    {
       


  

        public string BranchOffice { get; set; }
        public bool IsRejected { get; set; }

        public CashApprovalResponse ConvertToCashApprovalResponse(string BranchCode)
        { 
            return new CashApprovalResponse { 
                ApprovedMessage = this.ApprovedMessage, 
                IsApproved = DetermineApprovalStatus(), 
                Id = this.Id,
                CorrespondingBranchId = this.CorrespondingBranchId,
                ApprovedAmount = this.AmountApproved,
                BranchId = this.BranchId,
                BranchCode= BranchCode
            };  
        }

        public bool DetermineApprovalStatus()
        {
             bool result = false;
            if (this.IsApproved == true || this.IsRejected == false)
            {
                result = true;
            }
             return result;
        }
    }


    public class CashReplenimentRequestCompleteDto : CashReplenimentRequest
    {


        public string BankRecieptVirtualPath { get; set; }
        public string CorrespondingBranchId { get; set; }

        public string BranchOffice { get; set; }
        public bool IsRejected { get; set; }

        public CashApprovalResponse ConvertToCashApprovalResponse(string BranchCode)
        {
            return new CashApprovalResponse
            {
                ApprovedMessage = this.ApprovedMessage,
                IsApproved = DetermineApprovalStatus(),
                Id = this.Id,
                CorrespondingBranchId = this.CorrespondingBranchId,
                ApprovedAmount = this.AmountApproved,
                BranchId = this.BranchId,
                BranchCode = BranchCode
            };
        }

        public bool DetermineApprovalStatus()
        {
            bool result = false;
            if (this.IsApproved == true || this.IsRejected == false)
            {
                result = true;
            }
            return result;
        }
    }
}
