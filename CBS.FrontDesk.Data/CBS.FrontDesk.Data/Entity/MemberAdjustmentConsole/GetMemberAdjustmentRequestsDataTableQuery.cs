using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.MemberAdjustmentConsole
{
    /*
     {"BranchId":"","Status":"","CustomerId":"","RequestedBy":"","ApprovedBy":"","StartDate":null,"EndDate":null,"Options":{"searchValue":""},"options":{"draw":1,"start":0,"length":10,"skip":0,"pageSize":10,"searchValue":"","sortColumnName":"RequestedDate","sortColumnDirection":"desc"}}
     */
    public class GetMemberAdjustmentRequestsDataTableQuery 
    {
        public DataTableOptions Options { get; set; }

        public string BranchId { get; set; }
        public string Status { get; set; }/*
        public string MemberId { get; set; }*/
        public string CustomerId { get; set; }
        public string AccountId { get; set; }
        public string RequestedBy { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

       /* public GetMemberAdjustmentRequestsDataTableQuery(
            DataTableOptions options,
            string branchId = null,
            string status = null,
            string memberId = null,
            string accountId = null,
            string requestedBy = null,
            string approvedBy = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            Options = options;
            BranchId = branchId;
            Status = status;
            MemberId = memberId;
            AccountId = accountId;
            RequestedBy = requestedBy;
            ApprovedBy = approvedBy;
            StartDate = startDate;
            EndDate = endDate;
        }*/
    }
}
