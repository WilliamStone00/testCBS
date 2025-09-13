using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.BulkOperation
{
    public class BulkCashOperationFileDetailsData
    {

    }

    /// <summary>
    /// Query to retrieve paginated, filtered, and sortable bulk operation data for DataTable.
    /// </summary>
    public class GetBulkCashBulkOperationDataTableQuery
    {
        /// <summary>
        /// DataTable options containing pagination, sorting, and search parameters.
        /// </summary>
        public DataTableOptions DataTableOptions { get; set; }

    }
}
