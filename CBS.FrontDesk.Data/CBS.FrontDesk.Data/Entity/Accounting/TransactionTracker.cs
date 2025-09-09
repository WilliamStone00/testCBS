using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class PaginatedResource : ResourceParameter
    {
        public PaginatedResource() : base("TransactionDate")
        {
        }
        public string BranchId { get; set; }
        public bool IsByBranch { get; set; }
    }
    public class TransactionTrackerDatatableQuery
    {
        public DataTableOptions DataTableOptions { get; set; }
        public string Id { get; set; }
        public string CommandDataType { get; set; }
        public string CommandJsonObject { get; set; }
        public string TransactionReferenceId { get; set; }
        public bool HasPassed { get; set; }
        public int NumberOfRetry { get; set; }
        public DateTime DatePassed { get; set; }
        public DateTime TransactionDate { get; set; }
        public string DestinationUrl { get; set; }
        public string SourceUrl { get; set; }
        public string UserFullName { get; set; }
        public string BranchOffice { get; set; }
        public string BranchId { get; set; }
        public string BranchCode { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ErrorOrSuccessMessage { get; set; }
    }
    public class TransactionTracker
    {
        public PaginationMetadata PaginationMetadata { get; set; }
        public string Id { get; set; }
        public string CommandDataType { get; set; }
        public string CommandJsonObject { get; set; }
        public string TransactionReferenceId { get; set; }
        public bool HasPassed { get; set; }
        public int NumberOfRetry { get; set; }
        public DateTime DatePassed { get; set; }
        public DateTime TransactionDate { get; set; }
        public string DestinationUrl { get; set; }
        public string SourceUrl { get; set; }
        public string UserFullName { get; set; }
        public string BranchOffice { get; set; }
        public string BranchId { get; set; }
        public string BranchCode { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ErrorOrSuccessMessage { get; set; }
 
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

    }
    public class TransactionTrackerConfiguration
    {
        //
        public List<TransactionTracker> TransactionTrackers { get; set; } = new List<TransactionTracker>();
        public TransactionTracker TransactionTracker { get; set; } = new TransactionTracker();
        public QueryModel QueryModel { get; set; }
    }
}
