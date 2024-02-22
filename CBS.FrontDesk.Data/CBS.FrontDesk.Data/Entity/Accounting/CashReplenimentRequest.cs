using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class CashReplenimentRequest
    {
        public string Id { get; set; }
        public string ReferenceId { get; set; }
        public decimal AmountRequested { get; set; }
        public string RequestMessage { get; set; }
        [Required]
        public decimal AmountConfirm { get; set; }
        [Required]
        public string OpenOfDayOperationId { get; set; }
        public string IssuedBy { get; set; }
        public string IssuedDate { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public bool IsApproved { get; set; }
        public string CurrencyCode { get; set; }
        [Required]
        public string ApprovedMessage { get; set; }
        public string Status { get; set; }

        public CashApprovalResponse ConvertToCashApprovalResponse(bool Approved)
        { return new CashApprovalResponse { ApprovedMessage =this.ApprovedMessage, IsApproved=Approved,Id= this.Id }; }

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
                IsRejected = false
            };
        }
    }

    public class CashReplenimentRequestDto: CashReplenimentRequest
    { 
       
        public bool IsRejected { get; set; }

        public CashApprovalResponse ConvertToCashApprovalResponse()
        { return new CashApprovalResponse 
        { ApprovedMessage = this.ApprovedMessage, IsApproved = DetermineApprovalStatus(), Id = this.Id }; }

        public bool DetermineApprovalStatus()
        {
             bool result = false;
            if (this.IsApproved == true && this.IsRejected == false)
            {
                result = true;
            }
             return result;
        }
    }
}
