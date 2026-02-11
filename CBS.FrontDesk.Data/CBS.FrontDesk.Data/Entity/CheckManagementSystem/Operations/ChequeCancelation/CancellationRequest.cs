using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeCancelation
{
    // CancellationRequest.cs
    public class CancellationRequest
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string CheckBookId { get; set; }
        public string CheckBookLeaveId { get; set; }
        public string CancellationType { get; set; }
        // Example: "CheckBook", "CheckLeaf", "SingleCheck"
        public string Reason { get; set; }
        public string RequestedBy { get; set; }
        public string BranchId { get; set; }
        public string RequestToken { get; set; }
        public string Status { get; set; }
    }


    public class CancellationRequestQuery
    {
        public string Status { get; set; }
        public string BranchId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DataTableOptions DataTableOptions { get; set; }
    }
}
