using CBS.FrontDesk.Data.Entity.BulkOperation;
using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.ManualDailycollection
{
    public class FileUploadResponse
    {

        public string Id { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string CollectorAndUser { get; set; }
        public string FileUploadId { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalMembers { get; set; }
        public string UploadedBy { get; set; }    // Who uploaded the file
                                                  //  public DateTime UploadedAt { get; set; }           // Timestamp of upload

        // Navigation property
        public List<ManualDailyCollectionFileDetail> manualEntryDailyCollectorUploadListDtos { get; set; }
    }   

    public class collectorsDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Surename { get; set; }
    }
}
