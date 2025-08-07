using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.DailyCollectionEntities
{
    public class ManualEntryDailyCollectorUploadSummaryDto
    {
        public string FileUploadId { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalMembers { get; set; }
        public string BranchName { get; set; }
        public string UploadedBy { get; set; }
        public List<ManualEntryDailyCollectorUploadListDto> manualEntryDailyCollectorUploadListDtos { get; set; }

        public static ManualEntryDailyCollectorUploadSummaryDto CreateSampleManualEntryUploadSummary()
        {
            var uploadList = new List<ManualEntryDailyCollectorUploadListDto>
    {
        new ManualEntryDailyCollectorUploadListDto
        {
            MemberReference = "MEM001",
            MemberName = "John Smith",
            Amount = 150.50m,
            DailyCollectorName = "Alice Johnson",
            MemberBranchCode = "BR001",
            MemberBranchId = "1",
            MemberBranchName = "Downtown Branch"
        },
        new ManualEntryDailyCollectorUploadListDto
        {
            MemberReference = "MEM002",
            MemberName = "Sarah Wilson",
            Amount = 200.00m,
            DailyCollectorName = "Bob Martinez",
            MemberBranchCode = "BR002",
            MemberBranchId = "2",
            MemberBranchName = "Uptown Branch"
        },
        new ManualEntryDailyCollectorUploadListDto
        {
            MemberReference = "MEM003",
            MemberName = "Michael Brown",
            Amount = 175.25m,
            DailyCollectorName = "Carol Davis",
            MemberBranchCode = "BR001",
            MemberBranchId = "1",
            MemberBranchName = "Downtown Branch"
        },
        new ManualEntryDailyCollectorUploadListDto
        {
            MemberReference = "MEM004",
            MemberName = "Emma Thompson",
            Amount = 300.75m,
            DailyCollectorName = "Alice Johnson",
            MemberBranchCode = "BR003",
            MemberBranchId = "3",
            MemberBranchName = "West Side Branch"
        },
        new ManualEntryDailyCollectorUploadListDto
        {
            MemberReference = "MEM005",
            MemberName = "David Garcia",
            Amount = 125.00m,
            DailyCollectorName = "Bob Martinez",
            MemberBranchCode = "BR002",
            MemberBranchId = "2",
            MemberBranchName = "Uptown Branch"
        },
        new ManualEntryDailyCollectorUploadListDto
        {
            MemberReference = "MEM006",
            MemberName = "Lisa Anderson",
            Amount = 250.30m,
            DailyCollectorName = "Carol Davis",
            MemberBranchCode = "BR001",
            MemberBranchId = "1",
            MemberBranchName = "Downtown Branch"
        },
        new ManualEntryDailyCollectorUploadListDto
        {
            MemberReference = "MEM007",
            MemberName = "James Miller",
            Amount = 180.90m,
            DailyCollectorName = "Diana Rodriguez",
            MemberBranchCode = "BR004",
            MemberBranchId = "4",
            MemberBranchName = "East End Branch"
        },
        new ManualEntryDailyCollectorUploadListDto
        {
            MemberReference = "MEM008",
            MemberName = "Jennifer Lee",
            Amount = 220.45m,
            DailyCollectorName = "Alice Johnson",
            MemberBranchCode = "BR003",
            MemberBranchId = "3",
            MemberBranchName = "West Side Branch"
        },
        new ManualEntryDailyCollectorUploadListDto
        {
            MemberReference = "MEM009",
            MemberName = "Robert Taylor",
            Amount = 195.80m,
            DailyCollectorName = "Bob Martinez",
            MemberBranchCode = "BR002",
            MemberBranchId = "2",
            MemberBranchName = "Uptown Branch"
        },
        new ManualEntryDailyCollectorUploadListDto
        {
            MemberReference = "MEM010",
            MemberName = "Michelle White",
            Amount = 165.15m,
            DailyCollectorName = "Diana Rodriguez",
            MemberBranchCode = "BR004",
            MemberBranchId = "4",
            MemberBranchName = "East End Branch"
        }
    };

            return new ManualEntryDailyCollectorUploadSummaryDto
            {
                TotalAmount = uploadList.Sum(x => x.Amount),
                TotalMembers = uploadList.Count,
                BranchName = "Main Processing Center",
                UploadedBy = "System Administrator",
                manualEntryDailyCollectorUploadListDtos = uploadList
            };
        }

    }
    public class ManualEntryDailyCollectorUploadListDto
    {
        public string MemberReference { get; set; }
        public string MemberName { get; set; }
        public decimal Amount { get; set; }
        public string DailyCollectorName { get; set; }
        public string MemberBranchCode { get; set; }
        public string MemberBranchId { get; set; }
        public string MemberBranchName { get; set; }
    }
}
   
