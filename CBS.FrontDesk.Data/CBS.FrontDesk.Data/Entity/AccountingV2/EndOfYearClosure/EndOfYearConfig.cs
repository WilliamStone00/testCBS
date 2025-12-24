using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AccountingV2.EndOfYearClosure
{
    public class EndOfYearConfig
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsActive { get; set; }
        public string Category { get; set; }

        public string Comment { get; set; }
        public string AccountingYearId { get; set; }
    }

    public class EndOfYearTaskViewModel
    {
        public List<EndOfYearConfig> Tasks { get; set; } = new List<EndOfYearConfig>();

    }

    
    public class YearEndChecklistStatusQuery
    {
        public DataTableOptions Options { get; set; }
        public YearEndChecklistStatusQuery() { Options = new DataTableOptions(); }

        public string BranchId { get; set; } = null;
        public string AccountingYearId { get; set; }
        public string AdjustmentType { get; set; }
        public string Id { get; set; }


    }

}
