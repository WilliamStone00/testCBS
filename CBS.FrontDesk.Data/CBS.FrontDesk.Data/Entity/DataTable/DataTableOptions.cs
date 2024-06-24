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
        public int length { get; set; }
        public string sortColumnName { get; set; } // Change type to int?
        public string sortColumnDirection { get; set; }
        public string searchValue { get; set; }
        public int pageSize { get; set; }
        public int skip { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public string search { get; set; }
        public string sortDirection { get; set; }
    }
    public class CustomDataTable : ICustomDataTable
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public object data { get; set; }
        public DataTableOptions DataTableOptions { get; set; }

        public CustomDataTable(int draw, int recordsTotal, int recordsFiltered, object data, DataTableOptions dataTableOptions)
        {
            this.draw = draw;
            this.recordsTotal = recordsTotal;
            this.recordsFiltered = recordsFiltered;
            this.data = data;
            DataTableOptions = dataTableOptions;    
        }
    }


}
