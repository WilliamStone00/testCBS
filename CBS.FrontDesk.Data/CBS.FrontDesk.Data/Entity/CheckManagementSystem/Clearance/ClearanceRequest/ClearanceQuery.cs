using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.Clearance.ClearanceRequest
{
    public class ClearanceQuery
    {
        public string BranchId { get; set; } = null;
        public string OperationType { get; set; } = null;
        public string Status { get; set; } = null;
        public string StartDate { get; set; } = null;
        public string EndDate { get; set; } = null;

        public DataTableOptions Options { get; set; } = null;


        public ClearanceQuery()
        {
            Options = new DataTableOptions();
        }
    }
}
