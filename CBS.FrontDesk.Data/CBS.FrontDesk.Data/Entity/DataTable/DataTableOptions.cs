using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.DataTable
{
    public class DataTableOptions
    {
        public int start { get; set; }
        public string draw { get; set; }
        public int length { get; set; } = 1;
        public string sortColumnName { get; set; } // Change type to int?
        public string sortColumnDirection { get; set; }
        public string searchValue { get; set; }
        public int pageSize { get; set; } = 3000;
        public int skip { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public string search { get; set; }
        public string sortDirection { get; set; }
        public string lang { get; set; }
    }

    public class CustomDataTable : ICustomDataTable
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public object data { get; set; }
        public DataTableOptions DataTableOptions { get; set; }

        public CustomDataTable() { }

        public CustomDataTable(int draw, int recordsTotal, int recordsFiltered, object data, DataTableOptions dataTableOptions)
        {
            this.draw = draw;
            this.recordsTotal = recordsTotal;
            this.recordsFiltered = recordsFiltered;
            this.data = data;
            this.DataTableOptions = dataTableOptions;
        }
    }
    public class CustomDataTable2 
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public object data { get; set; }
        public DataTableOptions Options { get; set; }

        public CustomDataTable2() { }

        public CustomDataTable2(int draw, int recordsTotal, int recordsFiltered, object data, DataTableOptions dataTableOptions)
        {
            this.draw = draw;
            this.recordsTotal = recordsTotal;
            this.recordsFiltered = recordsFiltered;
            this.data = data;
            this.Options = dataTableOptions;
        }
    }

    public class CustomDataTable<T> : ICustomDataTable<T> where T : class 
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public T data { get; set; }
        public DataTableOptions DataTableOptions { get; set; }

        public CustomDataTable() { }

        public CustomDataTable(int draw, int recordsTotal, int recordsFiltered, T data, DataTableOptions dataTableOptions)
        {
            this.draw = draw;
            this.recordsTotal = recordsTotal;
            this.recordsFiltered = recordsFiltered;
            this.data = data;
            this.DataTableOptions = dataTableOptions;
        }
    }

    public class GetAuditTrailsDataTableQuery
    {
        public DataTableOptions DataTableOptions { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Feild { get; set; }
        public bool FilterByDatesOnly { get; set; }
    }

    public class GetDataTableDataQueryCommand
    {
        /// <summary>
        /// DataTable options containing pagination, sorting, and search parameters.
        /// </summary>
        public DataTableOptions DataTableOptions { get; set; }

        /// <summary>
        /// Optional filter to retrieve data from a specific branch.
        /// </summary>
        public string BranchId { get; set; }

        /// <summary>
        /// Optional filter to retrieve record by  specific record id.
        /// </summary>
        public string KeyId { get; set; }

        /// <summary>
        /// Optional data status filter. 
        
        /// Use "all" to include all statuses.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Optional start date to filter loans based on the data creation date.
        /// Only data created on or after this date will be included.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Optional end date to filter loans based on the data creation date.
        /// Only datas created on or before this date will be included.
        /// </summary>
        public DateTime? EndDate { get; set; }

        
        public string OtherStatus { get; set; }
    }

}

public class GetDataTableQuery
{
    public DataTableOptions DataTableOptions { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Feild { get; set; }
    public bool FilterByDatesOnly { get; set; }
}