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
        public string id { get; set; }
        public string customerId { get; set; }
        public string customerName { get; set; }
        public string accountNumber { get; set; }
        public string branchId { get; set; }
        public string branchName { get; set; }
        public string categoryId { get; set; }
        public string categoryName { get; set; }
        public int numberOfLeaves { get; set; }
        public string status { get; set; } // Active, Used, Cancelled, Blocked
        public DateTime? issueDate { get; set; }
        public DateTime? expiryDate { get; set; }
        public DateTime? createdDate { get; set; }
        public decimal feeAmount { get; set; }
        public string chequeSeriesStart { get; set; }
        public string chequeSeriesEnd { get; set; }
        public List<ChequeLeaf> ChequeLeaves { get; set; } = new List<ChequeLeaf>();
    }

    public class ChequeLeaf
    {
        public string id { get; set; }
        public string chequeBookId { get; set; }
        public int leafNumber { get; set; }
        public string chequeNumber { get; set; }
        public string status { get; set; } // Available, Used, Cancelled, Blocked
        public DateTime? usedDate { get; set; }
        public decimal? amount { get; set; }
        public string beneficiary { get; set; }
        public string remarks { get; set; }
        public DateTime? createdDate { get; set; }
        public DateTime? issueDate { get; set; }
        public DateTime? expiryDate { get; set; }
    }




    public class ChequeBookQuery
    {
        public ChequeBookQuery()
        {
            Options = new DataTableOptions();
        }

        public DataTableOptions Options { get; set; }

        // Basic identity / search
        public string CustomerName { get; set; }
        public string CustomerId { get; set; }
        public string BranchId { get; set; }

        // Status / category
        public string Status { get; set; }
        public string Category { get; set; }

        public int? NumberOfLeaves { get; set; }

        // Flags
        public bool? IsLost { get; set; }
        public bool? IsBlocked { get; set; }


        // Date filters
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

}