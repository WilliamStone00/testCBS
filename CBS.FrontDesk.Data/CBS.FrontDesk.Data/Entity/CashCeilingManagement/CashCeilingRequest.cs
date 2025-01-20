using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CashCeilingManagement
{
    public class CashCeilingRequest
    {
        public string Id { get; set; }
        public string TellerId { get; set; }
        public bool Status { get; set; }
        public string BranchId { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public decimal CashoutRequestAmount { get; set; }
        public string RequestType { get; set; }//Cash_To_Vault Or Subteller_Cash_To_PrimaryTeller
        public string Requetcomment { get; set; }

        public string RequestedBy { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }
        public DateTime InitializeDate { get; set; }
        public string ApprovedComment { get; set; }
        public string ApprovedStatus { get; set; }//Pending,Approved, Rejected
        public string TransactionReference { get; set; }
        public string Action { get; set; }
        public Teller Teller { get; set; }
        public AddCashCeilingRequestCommand AddCashCeilingRequestCommand { get; set; }
        public ValidationCashCeilingRequestCommand ValidationCashCeilingRequestCommand { get; set; }
        public CashCeilingRequest()
        {
            AddCashCeilingRequestCommand=new AddCashCeilingRequestCommand();
            ValidationCashCeilingRequestCommand=new ValidationCashCeilingRequestCommand();
            Teller=new Teller();
        }
    }

    public class AddCashCeilingRequestCommand
    {
        [Required(ErrorMessage = "Cashout request amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Cashout request amount must be greater than 0.")]
        public decimal CashoutRequestAmount { get; set; }

        [Required(ErrorMessage = "Request type is required.")]
        public string RequestType { get; set; }

        [StringLength(500, ErrorMessage = "Request comment cannot exceed 500 characters.")]
        public string Requetcomment { get; set; }
        public string Id { get; set; }
    }

    public class ValidationCashCeilingRequestCommand
    {
        public string Id { get; set; }

        [StringLength(500, ErrorMessage = "Approved comment cannot exceed 500 characters.")]
        public string ApprovedComment { get; set; }

        [Required(ErrorMessage = "Approval status is required.")]
        public string ApprovedStatus { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        public CurrencyNotesRequest CurrencyNote { get; set; }
    }

    // Enums for better readability
    public enum CashCeilingRequestType
    {
        Cash_To_Vault,
        Subteller_Cash_To_PrimaryTeller
    }

    public enum ApprovalStatus
    {
        Approved,
        Rejected,
        Pending
    }
    public class GetAllCashCeilingRequestsQuery
    {
        public string BranchId { get; set; } // Optional parameter to filter by branch
        public string Status { get; set; } // Optional parameter to filter by status
        public string UserId { get; set; }
        public string RequestType { get; set; }

    }


}
