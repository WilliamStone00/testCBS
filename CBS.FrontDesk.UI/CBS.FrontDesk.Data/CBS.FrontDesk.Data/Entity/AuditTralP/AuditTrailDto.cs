using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AuditTralP
{
    public class AuditTrailDto
    {
        public string Id { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; }
        public string MicroServiceName { get; set; }
        public string FullName { get; set; }
        public string UserID { get; set; }
        public string Level { get; set; }
        public string IPAddress { get; set; }
        public int? StatusCode { get; set; }
        public string StringifyObject { get; set; }
        public string DetailMessage { get; set; }
        public string BranchID { get; set; }
        public string BankID { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string CorrolationId { get; set; }
    }
    public class GetPagedAuditTrailQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
   
}
