using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.LossManagementSystem
{
    public class CheckbookDto
    {
        public string CheckBookId { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
    }

    public class CheckbookQueryloss
    {
        public DataTableOptions Options { get; set; }
        public CheckbookQueryloss()
        {
            // Ensure Options is never null
            Options = new DataTableOptions();
        }

        public string CustomerId { get; set; }
        public string CheckbookId { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
        public DateTime? IssueDateFrom { get; set; }
        public DateTime? IssueDateTo { get; set; }
    }
}
