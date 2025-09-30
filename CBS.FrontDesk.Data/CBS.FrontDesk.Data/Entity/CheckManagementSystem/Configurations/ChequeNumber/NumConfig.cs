using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.ChequeNumber
{
    public class NumConfig
    {
        public string Id { get; set; } = null;
        public string BranchId { get; set; }
        public string Name { get; set; }
        public string BankCode { get; set; }
        public int BankCodePosition { get; set; }
        public int BranchCodePosition { get; set; }
        public int YearPosition { get; set; }
        public int SeriaNumberPosition { get; set; }
        public bool AcceptSerialNumber { get; set; }
    }
    public class NumconfogQuery
    {
        public NumconfogQuery()
        {
            DataTableOptions = new DataTableOptions();
        }

        public DataTableOptions DataTableOptions { get; set; }

        public string Name { get; set; }
        public string BranchId { get; set; }
        public int BankCodePosition { get; set; }
        public int BranchCodePosition { get; set; }
        public int YearPosition { get; set; }
        public bool AcceptSerialNumber { get; set; }
        public int SeriaNumberPosition { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

}
