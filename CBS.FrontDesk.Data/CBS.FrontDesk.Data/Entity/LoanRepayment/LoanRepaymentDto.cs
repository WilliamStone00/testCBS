using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanRepayment
{
    public class LoanRefundDto
    {

        public string Id { get; set; } 
        public string TransactionCode { get; set; } 

        public string LoanId { get; set; } 
        public string CustomerId { get; set; } 

        public string MemberName { get; set; }  // ✅
        public string BranchId { get; set; } 
        public string BranchCode { get; set; }  // ✅
        public string BranchName { get; set; }  // ✅

        public string PaymentMethod { get; set; } 
        public string PaymentChannel { get; set; } 
        public bool IsCompleted { get; set; }
        public bool IsReversal { get; set; }

        public decimal Amount { get; set; }
        public decimal Principal { get; set; }
        public decimal Interest { get; set; }
        public decimal Penalty { get; set; }
        public decimal Tax { get; set; }
        public decimal Balance { get; set; }

        public DateTime DateOfPayment { get; set; }
        public DateTime CreatedDate { get; set; } // for default sorting if need
    }



    public class LoanRefundQuery
    {
        public DataTableOptions DataTableOptions { get; set; }
        public LoanRefundQuery() { DataTableOptions = new DataTableOptions(); }

        public string BranchId { get; set; }
        public string LoanId { get; set; }
        public string CustomerId { get; set; }
        public string TransactionCode { get; set; }
        public string PaymentChannel { get; set; }
        public string PaymentMethod { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsReversal { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? CheckPartialFlowRepayment { get; set; }
    }


    public class PartialBulkOperation
    {
        public string RefundId { get; set; }
        public string SavingProductId { get; set; }
    }
    public class BUlkRefundReconcilliation
    {
        public List<PartialBulkOperation> RefundCarrierBulkOperations { get; set; }
    }

    public class ExportRefundRequest
    {
        public List<LoanRefundDto> LoanRefund { get; set; }
        public ExportOptions ExportOptions { get; set; }
        public LoanRefundQuery Filters { get; set; }
    }
}
