using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public enum CashReplishmentRequestStatus
    {
        Pending,
        Awaiting_Branch_Transfer,
        Awaiting_Branch_CashClearing,
        Awaiting_Bank_CashOut,
        CashInFusion,
        Approved,
        PendingApproval,
        RedirectToBranchBCO,
        RedirectToBranchBD,
        RedirectToBranchBTB,
        Rejected,
        Awaiting_uploaded_bank_deposit_receipt,
 
        Completed
    }
    public enum CashRequisitionType
    {
        ORDER,
        REQUEST

    }
    public class CashReplenimentRequest
    {
        
                    public string CorrespondingBranch { get; set; }
        public string BranchOffice { get; set; }
        public string Id { get; set; }
        public string ReferenceId { get; set; }
        public decimal AmountRequested { get; set; }
        public string RequestMessage { get; set; }
        [Required]
        public string AmountApproved { get; set; }
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
        public bool HasAccount56 { get; set; }
        public bool IsOwner { get; set; }
        public string ParentCashReplenishId { get; set; } = "XXXXXXXX";
        public string ApprovalCode { get; set; }
        public string TempId1 { get; set; } = "Pendding";
        public string TempId2 { get; set; } = "Pendding";
        public string TempId3 { get; set; } = "AccountId";
        public string TempData { get; set; } 
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
                AmountApproved = this.AmountApproved,
                RequestMessage = this.RequestMessage,
                ReferenceId = this.ReferenceId,
                IssuedBy = this.IssuedBy,
                IssuedDate = this.IssuedDate,
                ApprovedBy = this.ApprovedBy,
                TempId1 = this.TempId1,
                TempId2 = this.TempId2,
                ApprovedDate = this.ApprovedDate,
                IsApproved = this.IsApproved,
                CurrencyCode = this.CurrencyCode,
                ApprovedMessage = this.ApprovedMessage,
                Status = this.Status,
                IsRejected = false,
                BranchId = this.BranchId,
                CashReplishmentRequestStatus = this.CashReplishmentRequestStatus, 
                CorrespondingBranchId = this.CorrespondingBranchId,
                CorrespondingBranch = this.CorrespondingBranch
            };
        }
    }
    //
    public class CashReplenimentRequestDto: CashReplenimentRequest
    {


        // to determine if request was created by a branch
        public bool IsOwner { get; set; }

        public string BranchOffice { get; set; }
        public bool IsRejected { get; set; }
 
        public CashApprovalResponse ConvertToCashApprovalResponse(string BranchCode)
        {
            return new CashApprovalResponse
            {
                ApprovedMessage = this.ApprovedMessage,
                IsApproved = this.IsApproved,
                Id = this.Id,
                Status = this.Status,
                CorrespondingBranchId = this.CorrespondingBranchId,
                ApprovedAmount = Convert.ToDecimal(this.AmountApproved),
                BranchId = this.BranchId,
                CashRequisitionType = this.CashRequisitionType,
                BranchCode = BranchCode,
                AccountId = this.TempId3
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
                ApprovedAmount = Convert.ToDecimal(this.AmountApproved),
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
