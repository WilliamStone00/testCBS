using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeBookListing
{
    //public class ChequeBook
    //{
    //    public string id { get; set; }
    //    public string customerName { get; set; }
    //    public int numberOfLeaves { get; set; }
    //    public string status { get; set; } // Active, Cancelled, Expired
    //    public string branchId { get; set; }
    //    public string branchName { get; set; }
    //    public DateTime issueDate { get; set; }
    //    public DateTime expiryDate { get; set; }
    //    public List<ChequeLeaf> ChequeLeaves { get; set; }
    //}

    //public class ChequeLeaf
    //{
    //    public string id { get; set; }
    //    public string chequeBookId { get; set; }
    //    public int leafNumber { get; set; }
    //    public string status { get; set; } // Available, Used, Blocked, Cancelled
    //    public DateTime issueDate { get; set; }
    //    public DateTime expiryDate { get; set; }
    //}

    //    public class ChequeBookDataTableQuery
    //    {
    //        public DataTableOptions DataTableOptions { get; set; }
    //        public string BranchId { get; set; }
    //        public DateTime? FromDate { get; set; }
    //        public DateTime? ToDate { get; set; }

    //        public ChequeBookDataTableQuery()
    //        {
    //            DataTableOptions = new DataTableOptions();
    //        }
    //    }

    public class ChequeBook
    {

        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
         public string BranchName { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string BranchId { get; set; }
        public string BankId { get; set; }
        public int NumberOfLeaves { get; set; }
        public int StartSerialNumber { get; set; }
        public int EndSerialNumber { get; set; }
        public int CurrentSerialNumber { get; set; }
        public int Current { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? IssuedDate { get; set; }
        public string IssuedBy { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public DateTime? ExpiratryDate { get; set; }
        public string LastUpdatedBy { get; set; }
        public string BlockedBy { get; set; }
        public bool IsBlocked { get; set; }
        public string ApproveBy { get; set; }
        public string BlockReasons { get; set; }
        public DateTime? BlockedDate { get; set; }
        public string CheckBookCategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool IsReissued { get; set; }
        public string ReplacementCheckBookId { get; set; }
        public bool NotifyOnClearance { get; set; }
        public bool NotifyOnPayment { get; set; }
        public bool NotifyOnAnyTransaction { get; set; }
        public bool IsPrinted { get; set; }
        public bool IsIssued { get; set; }
        public string StatusDescription { get; set; }
        public int UsedLeaves { get; set; }
        public int RemainingLeaves { get; set; }
        public decimal Balance { get; set; }
        public List<ChequeLeaf> Leaves { get; set; } = new List<ChequeLeaf>();

  //      public string id { get; set; }
  //      public string customerId { get; set; }
  //      public string customerName { get; set; }
  //      public string accountNumber { get; set; }
		//public string accountId { get; set; }
		//public string branchId { get; set; }
  //      public string branchName { get; set; }
  //      public string categoryId { get; set; }
  //      public string categoryName { get; set; }
  //      public int numberOfLeaves { get; set; }
  //      public string status { get; set; } // Active, Used, Cancelled, Blocked
  //      public DateTime? issueDate { get; set; }
  //      public DateTime? expiryDate { get; set; }
  //      public DateTime? createdDate { get; set; }
  //      public decimal feeAmount { get; set; }
  //      public string chequeSeriesStart { get; set; }
  //      public string chequeSeriesEnd { get; set; }
  //      public List<ChequeLeaf> ChequeLeaves { get; set; } = new List<ChequeLeaf>();

    }

    public class ChequeLeaf
    {
        public string Id { get; set; }
        public string CheckBookId { get; set; }
        public int SerialNumber { get; set; }
        public string Status { get; set; }
        public DateTime? IssuedDate { get; set; }
        public string IssuedTo { get; set; }
        public string TransactionId { get; set; }
        public DateTime? ClearedDate { get; set; }
        public DateTime? CancelledDate { get; set; }
        public string CancelledReason { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
        public string LastUpdatedBy { get; set; }
        public string Transaction { get; set; }  
    }




    public class ChequeBookQuery
    {
        public DataTableOptions Options { get; set; }
        public int? NumberOfLeaves { get; set; }
        public string BranchId { get; set; }
        public string CheckBookCategoryId { get; set; }
        public string BankId { get; set; }
        public string Status { get; set; }
        public string CustomerId { get; set; }
        //public bool? IsBlocked { get; set; }
        //public DateTime? CreatedDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public ChequeBookQuery()
        {
            Options = new DataTableOptions();
        }
    }

    public class CheckLeafQuery
    {
        public DataTableOptions Options { get; set; }
        public int NumberOfLeaves { get; set; }
        public string Status { get; set; }
        public string RequestId { get; set; }
        public string CustomerId { get; set; }
        public bool IsLost { get; set; }
        public DateTime LostDate { get; set; }
        public string LostReason { get; set; }
        public string LossReportedBy { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public CheckLeafQuery()
        {
            Options = new DataTableOptions();
        }
    }

      
    

}