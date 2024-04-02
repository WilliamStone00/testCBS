using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity
{



    public class Budget
    {
        public string Id { get; set; }
        public decimal TotalBudgetAmount { get; set; } // Total budget amount for the entire period
        public bool IsApproved { get; set; } // Indicates whether the budget is approved
        public DateTime ApprovalDate { get; set; } // Date when the budget was approved
        public string ApprovedBy { get; set; } //
        public DateTime IssuedDate { get; set; }
        public string LockedBy { get; set; }
        public bool IsLocked { get; set; } // Indicates whether the budget is locked from further modifications
        public DateTime LockDate { get; set; } // Date when the budget was locked
        public string Description { get; set; } // Description or purpose of the budget
        public string BudgetPeriodId { get; set; }
        public string UnitId { get; set; }
    }
}
