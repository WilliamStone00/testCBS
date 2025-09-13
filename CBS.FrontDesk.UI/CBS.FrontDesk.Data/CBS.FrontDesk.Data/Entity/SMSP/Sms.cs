using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.SMSP
{
    public class Sms
    {
        public string Id { get; set; }
        public string OperationType { get; set; }
        public string BranchId { get; set; }
        public string BankId { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string ServiceName { get; set; }
        public string MemberName { get; set; }
        public string MemberReference { get; set; }
        public string Message { get; set; }
        public string Msisdn { get; set; }
        public string Status { get; set; }
        public string Sender { get; set; }
        public string SendBy { get; set; }
        public decimal Cost { get; set; }
        public string StatusDescription { get; set; }
        public DateTime CreatedDate { get; set; }

    }
    public class GetAllSmsLogsDataTableQuery
    {
        public DataTableOptions Options { get; set; }
        public string OperationType { get; set; }
        public string BranchId { get; set; }
        public string MemberName { get; set; }
        public string MemberReference { get; set; }
        public string Msisdn { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string SendBy { get; set; }
        public string Status { get; set; }




    }
}
