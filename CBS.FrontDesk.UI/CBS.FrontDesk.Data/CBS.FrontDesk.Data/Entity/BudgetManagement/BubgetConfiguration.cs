using CBS.FrontDesk.Data.Entity.BudgetManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity
{
    public class BudgetConfiguration
    {
        public Budget Budget { get; set; } = new Budget();
        public BudgetCategory BudgetCategory { get; set; } = new BudgetCategory();
        public BudgetPeriod BudgetPeriod { get; set; }

        public OrganizationalUnit OrganizationalUnit { get; set; }

        public List<Budget> Budgets { get; set; } = new List<Budget>();
        public List<BudgetPeriod> BudgetPeriods { get; set; } = new List<BudgetPeriod> { };
        public List<BudgetCategory> BudgetCategories { get; set; } = new List<BudgetCategory> { };
        public List<OrganizationalUnit> OrganizationalUnits { get; set; } = new List<OrganizationalUnit> { };

        public string ServiceOption { get; set; }
        public string Action { get; set; }
        public string KEY { get; set; } = "KEY";
    }
}
